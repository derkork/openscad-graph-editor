using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;
using OpenScadGraphEditor.Widgets.ProjectTree;
using Serilog;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// This is our main editing interface for editing graphs of invokable things (functions/modules). It is the
    /// heavyweight alternative to <see cref="ScadGraph"/>. In contrast to <see cref="ScadGraph"/>,
    /// this graph never really changes the actual graph data but just invokes refactoring events in response to
    /// user input that will do the proper refactoring actions.
    /// </summary>
    public class ScadGraphEdit : GraphEdit
    {

        /// <summary>
        /// Emitted when refactorings are requested.
        /// </summary>
        public event Action<string, Refactoring[]> RefactoringsRequested;
        
        /// <summary>
        /// Emitted when the user requested copying nodes.
        /// </summary>
        public event Action<ScadGraphEdit, List<ScadNode>> CopyRequested;

        /// <summary>
        /// Emitted when the user requested cutting nodes.
        /// </summary>
        public event Action<ScadGraphEdit, List<ScadNode>> CutRequested;
        
        /// <summary>
        /// Emitted when the user requested pasting nodes. 
        /// </summary>
        public event Action<ScadGraphEdit, Vector2> PasteRequested;
        
        /// <summary>
        /// Emitted when the user right-clicks on a node.
        /// </summary>
        public event Action<RequestContext> NodePopupRequested;

        /// <summary>
        /// Emitted when the user requires the dialog to add a node.
        /// </summary>
        public event Action<RequestContext> AddDialogRequested;

        /// <summary>
        /// Emitted when the user requests editing the comment of a node.
        /// </summary>
        public event Action<RequestContext> EditCommentRequested;

        /// <summary>
        /// Emitted when the user requests help on a certain node.
        /// </summary>
        public event Action<RequestContext> HelpRequested;

        /// <summary>
        /// Called when data from any list entry is dropped.
        /// </summary>
        public event Action<ScadGraphEdit,ProjectTreeEntry, Vector2, Vector2> ItemDataDropped;
        
        private readonly HashSet<string> _selection = new HashSet<string>();
        private readonly Dictionary<string, ScadNodeWidget> _widgets = new Dictionary<string, ScadNodeWidget>();
        private ScadConnection _pendingDisconnect;
        public ScadGraph Graph { get; private set; }


        public void SelectNodes(List<ScadNode> nodes)
        {
            var set = nodes.Select(it => it.Id).ToHashSet();
            _widgets.Values.ForAll(it => it.Selected = set.Contains(it.BoundNode.Id));
            _selection.Clear();
            set.ForAll(it => OnNodeSelected(_widgets[it]));
        }


        public override void _Ready()
        {
            RightDisconnects = true;

            // we'll be handling all connection request ourselves, so we will per se allow
            // everything to be connected to everything else.
            var allPortTypes = Enum.GetValues(typeof(PortType)).Cast<PortType>().ToList();
            foreach (var from in allPortTypes)
            {
                foreach (var to in allPortTypes)
                {
                   AddValidConnectionType((int) from, (int) to);
                }
            }

            this.Connect("connection_request")
                .To(this, nameof(OnConnectionRequest));
            this.Connect("disconnection_request")
                .To(this, nameof(OnDisconnectionRequest));
            this.Connect("connection_to_empty")
                .To(this, nameof(OnConnectionToEmpty));
            this.Connect("connection_from_empty")
                .To(this, nameof(OnConnectionFromEmpty));
            this.Connect("node_selected")
                .To(this, nameof(OnNodeSelected));
            this.Connect("node_unselected")
                .To(this, nameof(OnNodeUnselected));
            this.Connect("delete_nodes_request")
                .To(this, nameof(OnDeleteSelection));
            this.Connect("popup_request")
                .To(this, nameof(OnPopupRequest));

            this.Connect("_end_node_move")
                .To(this, nameof(OnEndNodeMove));
        }

        public override bool CanDropData(Vector2 position, object data)
        {
            return data is ProjectTreeDragData;
        }

        public override void DropData(Vector2 position, object data)
        {
            if (data is ProjectTreeDragData itemListDragData)
            {
                ItemDataDropped?.Invoke(this, itemListDragData.Entry, GetGlobalMousePosition(), LocalToGraphRelative(position));
            }
        }

        private void CreateOrGet(ScadNode node)
        {
            if (!_widgets.TryGetValue(node.Id, out var widget))
            {
                widget = node is IHaveCustomWidget iUseCustomWidget
                    ? iUseCustomWidget.InstantiateCustomWidget()
                    : Prefabs.New<ScadNodeWidget>();

                // this is technically not needed but it would seem that the graph edit gets confused if you don't set
                // the name of the widget to something unique.
                widget.Name = node.Id;
                widget.LiteralToggled += (port, enabled) =>
                    PerformRefactorings( "Toggle literal", new ToggleLiteralRefactoring(Graph, node, port, enabled));
                widget.LiteralValueChanged += (port, value) =>
                    PerformRefactorings("Set literal value", new SetLiteralValueRefactoring(Graph, node, port, value));
                widget.SizeChanged += size =>
                    PerformRefactorings("Change size", new ChangeNodeSizeRefactoring(Graph, node, size));

                _widgets[node.Id] = widget;
                widget.MoveToNewParent(this);
            }
            
            widget.BindTo(Graph, node);
        }

        private void OnEndNodeMove()
        {
            // all nodes which are currently selected have moved, so we need to send a refactoring request for them.
            PerformRefactorings("Move node(s)",
                GetSelectedNodes()
                    .Select(it => new ChangeNodePositionRefactoring(Graph, it, _widgets[it.Id].Offset)));

        }
        
        public void FocusEntryPoint()
        {
            var entryPoint = Graph.GetAllNodes().FirstOrDefault(it => it is EntryPoint);
            if (entryPoint != null)
            {
                var widget = _widgets[entryPoint.Id];
                // move it somewhere top left
                ScrollOffset = widget.Offset - new Vector2(100, 100);
            }
        }
        
        public void FocusNode(ScadNode node)
        {
           var widget = _widgets[node.Id];
           var middle = RectSize / 2;
           
           // calculate the offset we need to move to get the widget into the middle.
           ScrollOffset = widget.Offset - (middle * Zoom);
           widget.Flash();
        }
        

        public void Render(ScadGraph graph)
        {
            Graph = graph;
            
            // rebuild the nodes
            foreach (var node in Graph.GetAllNodes())
            {
                CreateOrGet(node); 
            }

            // redo the connections
            ClearConnections();
            
            foreach (var connection in Graph.GetAllConnections())
            {
                // do not show connections to wireless reroutes by default, unless the reroute node is currently selected
                if (connection.To is RerouteNode rerouteNode && rerouteNode.IsWireless && !_selection.Contains(connection.To.Id))
                {
                    continue; 
                }

                // connection contains ScadNode ids but we need to connect widgets, so first we need to find the 
                // the widget and then connect these widgets.
                ConnectNode(_widgets[connection.From.Id].Name, connection.FromPort, _widgets[connection.To.Id].Name, connection.ToPort);
            }
            
            // finally destroy all the widgets that are not in the graph anymore
            // set of Ids of nodes that are in the graph
            var nodeIds = graph.GetAllNodes().Select(n => n.Id).ToHashSet();
            // set of Ids of our widgets
            var widgetIds = _widgets.Keys.ToHashSet();
            // difference between the two sets
            var idsToDestroy = widgetIds.Except(nodeIds).ToList();
            // destroy all the widgets that are not in the graph anymore
            foreach (var id in idsToDestroy)
            {
                _widgets[id].RemoveAndFree();
                _widgets.Remove(id);
            }
        }
        
        /// <summary>
        /// Calculates the graph relative position for any global coordinate taking zoom and scroll offset into account.
        /// </summary>
        private Vector2 GlobalToGraphRelative(Vector2 globalPosition)
        {
            var localPosition = globalPosition - RectGlobalPosition;
            return LocalToGraphRelative(localPosition);
        }

        /// <summary>
        /// Calculates the graph relative position for any local coordinate (relative to this editor) taking zoom and scroll offset into account.
        /// </summary>
        private Vector2 LocalToGraphRelative(Vector2 localPosition)
        {
            var relativePosition = localPosition;
            relativePosition += ScrollOffset;
            relativePosition /= Zoom;
            return relativePosition;
        }
        

        private void OnPopupRequest(Vector2 position)
        {
            var relativePosition = GlobalToGraphRelative(position);

            var matchingWidgets = _widgets.Values
                .FirstOrDefault(it => new Rect2(it.Offset, it.RectSize).HasPoint(relativePosition));

            if (matchingWidgets == null)
            {
                // right-click in empty space yields you the add dialog
                AddDialogRequested?.Invoke(RequestContext.ForPosition(Graph, relativePosition));
                return;
            }

            NodePopupRequested?.Invoke(RequestContext.ForNode(Graph, position, matchingWidgets.BoundNode));
            
        }


        public override void _GuiInput(InputEvent evt)
        {
            if (evt is InputEventMouseButton mouseButtonEvent && !mouseButtonEvent.Pressed && _pendingDisconnect != null)
            {
                Log.Debug("Resolving pending disconnect");
                PerformRefactorings("Remove connection", new DeleteConnectionRefactoring(_pendingDisconnect));
                _pendingDisconnect = null;
                return;
            }

            if (evt.IsCopy())
            {
                CopyRequested?.Invoke(this, GetSelectedNodes().ToList());
                return;
            }
            

            if (evt.IsCut())
            {
                CutRequested?.Invoke(this, GetSelectedNodes().ToList());
                return;
            }
            
            if (evt.IsPaste())
            {
                // get the offset over the mouse position
                var globalRect = GetGlobalRect();
                var globalMousePosition = GetGlobalMousePosition();

                // default paste position
                var pastePosition = ScrollOffset + new Vector2(100, 100);
                
                if (globalRect.HasPoint(globalMousePosition))
                {
                    // if the mouse is inside the graph, paste the nodes at the mouse position
                    pastePosition = GlobalToGraphRelative(globalMousePosition);

                }
                
                PasteRequested?.Invoke(this, pastePosition);
                return;
            }

            if (evt.IsEditComment() && _selection.Count == 1)
            {
                var selectedNode = Graph.GetAllNodes().First(it => it.Id == _selection.First());
                EditCommentRequested?.Invoke(RequestContext.ForNode(Graph, selectedNode.Offset, selectedNode));
                return;
            }

            if (evt.IsShowHelp() && _selection.Count == 1)
            {
                var selectedNode = Graph.GetAllNodes().First(it => it.Id == _selection.First());
                HelpRequested?.Invoke(RequestContext.ForNode(Graph, selectedNode.Offset, selectedNode));
                return;
            }
            
            if (evt.IsStraighten())
            {
                StraightenSelection();
            }
        }

        private void StraightenSelection()
        {
            // all selected nodes 
            var selectedNodes = GetSelectedNodes().ToHashSet();

            // relevant connections, these are all connections between the selected nodes
            var relevantConnections = Graph.GetAllConnections()
                .Where(it => selectedNodes.Contains(it.From) && selectedNodes.Contains(it.To))
                // also exclude connections which have a wireless node as target, as we don't layout those
                .Where(it => !(it.To is RerouteNode rerouteNode && rerouteNode.IsWireless))
                .ToList();

            // first up remove all nodes which do not have any connections to any other selected node, there is nothing to
            // straighten for them.
            selectedNodes.RemoveWhere(it => !relevantConnections.Any(connection => connection.InvolvesNode(it)));
            
            // now we have two sets, one is the set of nodes that are starting points for straightening, the other
            // is the set of nodes we have not yet laid out.
            var nodesToStraighten = selectedNodes.ToHashSet();
            var startingPoints = new HashSet<ScadNode>();

            // We are going to move the nodes around but only do so after the 
            // straightening refactoring is run. However some calculations depend on knowing where
            // the node will be after the refactoring. So we make ourselves a little lookup table where
            // we have the current node positions by id, which we overwrite during the layout process
            var nodePositions = new Dictionary<string, Vector2>(); 
            nodesToStraighten.ForAll(it => nodePositions[it.Id] = it.Offset);
            
            var refactorings = new List<Refactoring>();
            
            // now while we still have nodes in the set
            while (nodesToStraighten.Count > 0)
            {
                // first check if we have any starting points, if not, take the rightmost node we still have to 
                // straighten and use that one as a starting point.
                if (!startingPoints.Any())
                {
                    var newStartingPoint = nodesToStraighten.OrderByDescending(it => it.Offset.x).First();
                    startingPoints.Add(newStartingPoint);
                    nodesToStraighten.Remove(newStartingPoint);
                }
                
                GdAssert.That(startingPoints.Any(), "Should have at least one starting point");
                
                // now pop a node from the starting points and go from there
                var currentNode = startingPoints.First();
                startingPoints.Remove(currentNode);

                if (currentNode is RerouteNode rerouteNode && rerouteNode.IsWireless)
                {
                    continue; // a wireless node has no incoming connections that could be straightened
                }

                var currentNodeConnections = relevantConnections
                    .Where(it => it.To == currentNode)
                    .OrderBy(it => it.ToPort);
                
                // now walk over the connections and see if we can straighten them
                foreach (var connection in currentNodeConnections)
                {
                    if (!nodesToStraighten.Contains(connection.From))
                    {
                        // the connection is to a node we already moved, so go to the next one.
                        continue;
                    }
                    // good, we can straighten this connection
                    // to do this, we need to get the port offset of the right side of the connection
                    // and then move the left side of the connection to that offset
                    var rightSideWidget = _widgets[connection.To.Id];
                    var leftSideWidget = _widgets[connection.From.Id];

                    var rightOffset = nodePositions[connection.To.Id];
                    var rightConnectorPosition = GlobalToGraphRelative(rightSideWidget.GetConnectionInputPosition(connection.ToPort)) + rightOffset;
                    var leftOffset = nodePositions[connection.From.Id];
                    var leftConnectorPosition = GlobalToGraphRelative(leftSideWidget.GetConnectionOutputPosition(connection.FromPort)) + leftOffset;
                        
                    // we only move the left node and we only change the y axis
                    var deltaY = rightConnectorPosition.y - leftConnectorPosition.y;
                    
                    // only run a refactoring if we actually need to
                    if (Mathf.Abs(deltaY) > 0.001f)
                    {
                        var newLeftOffset = new Vector2(leftOffset.x,  leftOffset.y + deltaY);
                        // write it down into our intermediate node position lookup table
                        nodePositions[connection.From.Id] = newLeftOffset;
                        refactorings.Add(new ChangeNodePositionRefactoring(Graph, connection.From, newLeftOffset));
                    }
                    
                    // now we can remove the node from the set of nodes to straighten but it can still be a starting
                    // point for other nodes
                    nodesToStraighten.Remove(connection.From);
                    startingPoints.Add(connection.From);
                }
            }

            if (refactorings.Any())
            {
                PerformRefactorings("Straighten connections", refactorings);
            }
            else
            {
                NotificationService.ShowNotification("Everything is already straightened");
            }
        }

        private IEnumerable<ScadNode> GetSelectedNodes()
        {
            return _selection.Select(it => _widgets[it].BoundNode);
        }

        private void OnDisconnectionRequest(string fromWidgetName, int fromSlot, string toWidgetName, int toSlot)
        {
            Log.Debug("Disconnect request");
            var connection = new ScadConnection(Graph,ScadNodeForWidgetName(fromWidgetName), fromSlot,
                ScadNodeForWidgetName(toWidgetName), toSlot);
            
            // the disconnect is not done until the user has released the mouse button, so in case this is called
            // while the mouse is still down, just visually disconnect, but don't do a refactoring yet.
            if (Input.IsMouseButtonPressed((int) ButtonList.Left))
            {
                DisconnectNode(fromWidgetName, fromSlot, toWidgetName, toSlot);
                _pendingDisconnect = connection;
            }
        }

        private void OnConnectionToEmpty(string fromWidgetName, int fromPort, Vector2 releasePosition)
        {
            var context = RequestContext.FromPort(Graph, LocalToGraphRelative(releasePosition), ScadNodeForWidgetName(fromWidgetName), fromPort);
            AddDialogRequested?.Invoke(context);
        }

        private void OnConnectionFromEmpty(string toWidgetName, int toPort, Vector2 releasePosition)
        {
            
            var context = RequestContext.ToPort(Graph, LocalToGraphRelative(releasePosition), ScadNodeForWidgetName(toWidgetName), toPort);
            AddDialogRequested?.Invoke(context);
        }

        private void OnNodeSelected(ScadNodeWidget node)
        {
            _selection.Add(node.BoundNode.Id);
            
            // if the node was a wireless node, then temporarily show its wireless connection
            if (node.BoundNode is RerouteNode rerouteNode && rerouteNode.IsWireless)
            {
                var relevantConnections = Graph.GetAllConnections()
                    .Where(it => it.To == node.BoundNode);

                foreach (var connection in relevantConnections)
                {
                    ConnectNode(_widgets[connection.From.Id].Name, connection.FromPort, _widgets[connection.To.Id].Name, connection.ToPort);
                }
            }

        }

        private void OnNodeUnselected(ScadNodeWidget node)
        {
            _selection.Remove(node.BoundNode.Id);
            
            // if the node was a wireless node, then hide its wireless connections
            if (node.BoundNode is RerouteNode rerouteNode && rerouteNode.IsWireless)
            {
                var relevantConnections = Graph.GetAllConnections()
                    .Where(it => it.To == node.BoundNode);
                foreach(var connection in relevantConnections)
                {
                    DisconnectNode(_widgets[connection.From.Id].Name, connection.FromPort, _widgets[connection.To.Id].Name, connection.ToPort);
                }
            }
        }

        private void OnDeleteSelection()
        {
            var refactorings = 
                GetSelectedNodes()
                    .Select(it => new DeleteNodeRefactoring(Graph, it))
                    .ToList();
            _selection.Clear();
            PerformRefactorings("Delete selection",  refactorings);
        }


        private void OnConnectionRequest(string fromWidgetName, int fromPort, string toWidgetName, int toPort)
        {
            var connection = new ScadConnection(Graph, ScadNodeForWidgetName(fromWidgetName), fromPort,
                ScadNodeForWidgetName(toWidgetName), toPort);
            if (_pendingDisconnect != null)
            {
                if (_pendingDisconnect.RepresentsSameAs(connection))
                {
                    Log.Debug("Re-connected pending node");
                    ConnectNode(fromWidgetName, fromPort, toWidgetName, toPort);
                    _pendingDisconnect = null;
                    return;
                }
            }
            
            
            var refactorings = new AddConnectionRefactoring(connection);
            PerformRefactorings("Create connection", refactorings);
        }


        private ScadNode ScadNodeForWidgetName(string widgetName)
        {
            return this.AtPath<ScadNodeWidget>(widgetName).BoundNode;
        }


        private void PerformRefactorings(string description, params Refactoring[] refactorings)
        {
            PerformRefactorings(description, (IEnumerable<Refactoring>) refactorings);
        }

        private void PerformRefactorings(string description, IEnumerable<Refactoring> refactorings)
        {
            var refactoringsAsArray = refactorings.ToArray();
            if (refactoringsAsArray.Length == 0)
            {
                return;
            }
            
            RefactoringsRequested?.Invoke(description, refactoringsAsArray);
        }

    }

}