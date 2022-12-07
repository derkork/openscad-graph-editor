using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using KdTree;
using KdTree.Math;
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
        /// Emitted when the user requested duplicating nodes.
        /// </summary>
        public event Action<ScadGraphEdit, List<ScadNode>, Vector2> DuplicateRequested;

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
        /// <summary>
        /// Lookup tree for finding the closest connection to a given point.
        /// </summary>
        private  KdTree<float, (ScadConnection Connection,List<Vector2> BezierPoints)> _connectionTree = 
            new KdTree<float, (ScadConnection Connection,List<Vector2> BezierPoints)>(2, new FloatMath());
        private ScadConnection _pendingDisconnect;
        public ScadGraph Graph { get; private set; }
        
        private Control _connectionHighlightLayer;
        
        private List<Vector2> _connectionHighlightPoints;
        private ScadConnection _highlightedConnection;


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
            
            this.Connect("draw")
                .To(this, nameof(BuildConnectionLookupTree));


            _connectionHighlightLayer = this.WithName<Control>("CLAYER");
            _connectionHighlightLayer
                .Connect("draw")
                .To(this, nameof(OnConnectionHighlightLayerDraw));
            
            this.Connect("scroll_offset_changed")
                .To(this, nameof(OnScrollOffsetChanged));
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
                if (connection.To is RerouteNode {IsWireless: true} && !_selection.Contains(connection.To.Id))
                {
                    continue; 
                }

                // connection contains ScadNode ids but we need to connect widgets, so first we need to find the 
                // the widget and then connect these widgets.
                ConnectNode(_widgets[connection.From.Id].Name, connection.FromPort, _widgets[connection.To.Id].Name, connection.ToPort);
            }
            
            // build the lookup tree
            BuildConnectionLookupTree();

            // highlight any bound nodes
            HighlightBoundNodes();
            
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
                // also remove them from the selection if they were in 
                _selection.Remove(id);
            }
        }

        private void BuildConnectionLookupTree()
        {
            _connectionTree =
                new KdTree<float, (ScadConnection Connection, List<Vector2> BezierPoints)>(2, new FloatMath());
            foreach (var connection in Graph.GetAllConnections())
            {
                var connectionPoints = GetConnectionPoints(connection);
                var bezierPoints = BakeBezierLine(connectionPoints.Start, connectionPoints.End, 1);
                foreach (var bezierPoint in bezierPoints)
                {
                    _connectionTree.Add(new[] {bezierPoint.x, bezierPoint.y}, (connection, bezierPoints));
                }
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
            
            if (_highlightedConnection != null)
            {
                // a right click on a connection will in every case delete the connection, so first
                // make it no longer highlighted but keep the reference.
                var toDelete = _highlightedConnection;
                _highlightedConnection = null;
                _connectionHighlightPoints = null;
                
                // if shift is pressed we want to break the connection and insert a reroute node
                // if additionally ctrl is pressed we want to break the connection and insert a wireless reroute node
                if (KeyMap.IsShiftPressed())
                {
                    var rerouteNode = NodeFactory.Build<RerouteNode>();
                    if (KeyMap.IsCmdOrControlPressed())
                    {
                        rerouteNode.IsWireless = true;
                    }
                    rerouteNode.Offset = relativePosition;
                    
                    PerformRefactorings("Insert reroute node",
                            // first, delete the connection
                            new DeleteConnectionRefactoring( toDelete),
                            // then insert the reroute node and connect it to the old connection's target
                            new AddNodeRefactoring(Graph, rerouteNode, toDelete.To, PortId.Input(toDelete.ToPort)),
                            // and add a new connection from the old connection's source to the reroute node
                            new AddConnectionRefactoring(new ScadConnection(Graph, toDelete.From, toDelete.FromPort, rerouteNode, 0))
                            );
                    return;
                }
 
                // otherwise we want to delete the connection
                PerformRefactorings("Delete connection", new DeleteConnectionRefactoring(toDelete));
                return;
            }

            

            if (!TryGetNodeAtPosition(relativePosition, out var node))
            {
                // right-click in empty space yields you the add dialog
                AddDialogRequested?.Invoke(RequestContext.ForPosition(Graph, relativePosition));
                return;
            }

            NodePopupRequested?.Invoke(RequestContext.ForNode(Graph, position, node));
            
        }

        private bool TryGetNodeAtPosition(Vector2 relativePosition, out ScadNode node)
        {
            var firstMatchingWidget = _widgets.Values
                .FirstOrDefault(it => new Rect2(it.Offset, it.RectSize).HasPoint(relativePosition));

            node = firstMatchingWidget?.BoundNode;
            return node != null;
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

            if (evt is InputEventMouseMotion mouseMotionEvent)
            {
                // if the mouse is over a node, don't do anything
                var mousePositionRelativeToTheGraph = GlobalToGraphRelative(mouseMotionEvent.GlobalPosition);
                if (TryGetNodeAtPosition(mousePositionRelativeToTheGraph, out _))
                {
                    ClearHighlightedConnection();
                    return;
                }
                
                // find the closest connection to the mouse
                // we use * zoom because GlobalToGraphRelative will give us the coordinates in graph space and 
                // we need to convert them to screen space but relative to the graph widget.
                var mousePosition = mousePositionRelativeToTheGraph * Zoom;
                
                var closest = _connectionTree.GetNearestNeighbours(new []{mousePosition.x, mousePosition.y}, 1);
                if (closest.Length <= 0)
                {
                    ClearHighlightedConnection();
                    return;
                } 
                
                
                // are we close enough to the connection?
                var distance = new Vector2(closest[0].Point[0], closest[0].Point[1]).DistanceTo(mousePosition);
                if (distance > 20)
                {
                    ClearHighlightedConnection();
                    return;
                }
                
                // no need to redraw, if the connection is the same
                if (_highlightedConnection == closest[0].Value.Connection)
                {
                    return;
                }
                
                _highlightedConnection = closest[0].Value.Connection;
                _connectionHighlightPoints = closest[0].Value.BezierPoints;
                _connectionHighlightLayer.Update();
            }

            if (evt.IsCopy())
            {
                CopyRequested?.Invoke(this, GetSelectedNodes().ToList());
                return;
            }

            if (evt.IsDuplicate())
            {
                var pastePosition = GetPastePosition();

                DuplicateRequested?.Invoke(this, GetSelectedNodes().ToList(), pastePosition);
            }
            
            if (evt.IsCut())
            {
                CutRequested?.Invoke(this, GetSelectedNodes().ToList());
                return;
            }
            
            if (evt.IsPaste())
            {
                var pastePosition = GetPastePosition();

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

            if (evt.IsAlignLeft())
            {
                AlignSelectionLeft();
            }
            
            if (evt.IsAlignRight())
            {
                AlignSelectionRight();
            }
            
            if (evt.IsAlignTop())
            {
                AlignSelectionTop();
            }
            
            if (evt.IsAlignBottom())
            {
                AlignSelectionBottom();
            }
        }

        private void ClearHighlightedConnection()
        {
            if (_highlightedConnection == null)
            {
                return;
            }
            _highlightedConnection = null;
            _connectionHighlightPoints = null;
            _connectionHighlightLayer.Update();
        }

        private Vector2 GetPastePosition()
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

            return pastePosition;
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

        private void AlignSelectionLeft()
        {
            AlignSelection("Align left", 
                () => GetSelectedNodes().Min(it  => it.Offset.x),
                (it, position) => new Vector2(position, it.Offset.y));
        }

        private void AlignSelectionRight()
        {
            AlignSelection("Align right", 
                () => GetSelectedNodes().Max(it  => it.Offset.x + _widgets[it.Id].RectSize.x),
                (it, position) => new Vector2(position - _widgets[it.Id].RectSize.x, it.Offset.y));
        }
        
        private void AlignSelectionTop()
        {
            AlignSelection("Align top", 
                () => GetSelectedNodes().Min(it  => it.Offset.y),
                (it, position) => new Vector2(it.Offset.x, position));
        }
        
        private void AlignSelectionBottom()
        {
            AlignSelection("Align bottom", 
                () => GetSelectedNodes().Max(it  => it.Offset.y),
                (it, position) => new Vector2(it.Offset.x, position));
        }
        
        private void AlignSelection(string humanReadableName, Func<float> getPosition, Func<ScadNode, float, Vector2> getOffset)
        {
            if (GetSelectedNodes().Count() < 2)
            {
                NotificationService.ShowNotification("Select at least two nodes to align.");
                return;
            }
            
            var alignmentPosition = getPosition();
            var refactorings =
                GetSelectedNodes()
                    .Select(it =>
                        new ChangeNodePositionRefactoring(Graph, it, getOffset(it, alignmentPosition)));

            PerformRefactorings(humanReadableName, refactorings);
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
            HighlightBoundNodes();
        }

        private void HighlightBoundNodes()
        {
            var boundNodes = Graph.GetAllNodes().OfType<IAmBoundToOtherNode>().ToList();
            
            foreach (var node in boundNodes)
            {
                var partnerNodeId = node.OtherNodeId;
                var partnerNodeWidget = _widgets[partnerNodeId];
                // if the node's partner node is not currently selected, but the node is selected then highlight the node's partner node, otherwise clear it.
                if (!_selection.Contains(partnerNodeId) && _selection.Contains(((ScadNode)node).Id))
                {
                    partnerNodeWidget.Modulate = Colors.Yellow;
                }
                else
                {
                    partnerNodeWidget.Modulate = Colors.White;
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
            
            HighlightBoundNodes();
        }

        // Godot 3.4
        private void OnDeleteSelection()
        {
            var refactorings = 
                GetSelectedNodes()
                    .Select(it => new DeleteNodeRefactoring(Graph, it, KeyMap.IsKeepConnectionsPressed()))
                    .ToList();
            _selection.Clear();
            PerformRefactorings("Delete selection",  refactorings);
        }

        // Godot 3.5
        private void OnDeleteSelection([UsedImplicitly] Node[] _)
        {
            OnDeleteSelection();
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

        private (Vector2 Start, Vector2 End) GetConnectionPoints(ScadConnection connection)
        {
            var fromNode = _widgets[connection.From.Id];
            var toNode = _widgets[connection.To.Id];
            var fromPort = fromNode.GetConnectionOutputPosition(connection.FromPort) + fromNode.Offset * Zoom;
            var toPort = toNode.GetConnectionInputPosition(connection.ToPort) + toNode.Offset * Zoom;
            return (fromPort, toPort);
        }
        
        
        /// <summary>
        /// Bezier interpolation function for the connection lines. This is copied straight from the Godot source code.
        /// </summary>
        private static Vector2 BezierInterpolate(float t, Vector2 start, Vector2 control1, Vector2 control2, Vector2 end)
        {
            var omt = 1.0f - t;
            var omt2 = omt * omt;
            var omt3 = omt2 * omt;
            var t2 = t * t;
            var t3 = t2 * t;

            return start * omt3 + control1 * omt2 * t * 3.0f + control2 * omt * t2 * 3.0f + end * t3;
        }
   
        /// <summary>
        /// Segment baking function for the connection lines. This is copied straight from the Godot source code + some modifications.
        /// </summary>
        private static void BakeSegment2D(List<Vector2> points, float pBegin, float pEnd,
            Vector2 pA, Vector2 pOut, Vector2 pB, Vector2 pIn, int pDepth, int pMinDepth, int pMaxDepth,
            float pTol, float pMaxLength, ref int lines)
        {
            var mp = pBegin + (pEnd - pBegin) * 0.5f;
            var beg = BezierInterpolate(pBegin, pA, pA + pOut, pB + pIn, pB);
            var mid = BezierInterpolate(mp, pA, pA + pOut, pB + pIn, pB);
            var end = BezierInterpolate(pEnd, pA, pA + pOut, pB + pIn, pB);
            
            var na = (mid - beg).Normalized();
            var nb = (end - mid).Normalized();
            var dp = Mathf.Rad2Deg(Mathf.Acos(na.Dot(nb)));
            
            // the maxLength check is to ensure we don't get longer straight segments of lines because we want 
            // to keep the points that make up the lines evenly spaced, so our KDTree can find the closest point
            // to the mouse cursor and we don't get "holes" in the detection when the mouse is over a straight line
            // that technically only needs two points to be drawn. We insert additional points this way so
            // if the line is longer the algorithm can still detect it if the mouse cursor is in the middle of it.
            if (pDepth >= pMinDepth && (dp < pTol || pDepth >= pMaxDepth) && (beg - end).Length() < pMaxLength)
            {
                points.Add(end);
                lines++;
            }
            else
            {
                BakeSegment2D(points, pBegin, mp, pA, pOut, pB, pIn, pDepth + 1, pMinDepth, pMaxDepth, pTol, pMaxLength, ref lines);
                BakeSegment2D(points, mp, pEnd, pA, pOut, pB, pIn, pDepth + 1, pMinDepth, pMaxDepth, pTol, pMaxLength, ref lines);
            }
        }

        /// <summary>
        /// Builds a bezier curve from the given points and returns a list of points that make up the curve.
        /// </summary>
        private List<Vector2> BakeBezierLine(Vector2 fromPoint, Vector2 toPoint, float bezierRatio)
        {
            //cubic bezier code
            var diff = toPoint.x - fromPoint.x;
            var cpLen = GetConstant("bezier_len_pos") * bezierRatio;
            var cpNegLen = GetConstant("bezier_len_neg") * bezierRatio;

            var cpOffset = diff > 0 
                ? Mathf.Min(cpLen, diff * 0.5f) 
                : Mathf.Max(Mathf.Min(cpLen - diff, cpNegLen), -diff * 0.5f);

            var c1 = new Vector2(cpOffset * Zoom, 0);
            var c2 = new Vector2(-cpOffset * Zoom, 0);

            var lines = 0;

            var points = new List<Vector2> {fromPoint};
            BakeSegment2D(points, 0, 1, fromPoint, c1, toPoint, c2, 0, 3, 9, 3, 20, ref lines);
            points.Add(toPoint);
            
            return points;
        }
        
        private void OnScrollOffsetChanged([UsedImplicitly] Vector2 _)
        {
            // when the scroll offset/zoom is changed our previously highlighted connection is no longer valid
            // so we need to clear it, otherwise we'll get an artifact where the connection is still highlighted
            // in its old position and scale
            _highlightedConnection = null;
            _connectionHighlightPoints = null;
        }

        private void OnConnectionHighlightLayerDraw()
        {
            if (_highlightedConnection == null)
            {
                return;
            }
            _connectionHighlightLayer.DrawPolyline(_connectionHighlightPoints.ToArray(), new Color(1,1,1, 0.5f), 5, true);
        }

    }

}