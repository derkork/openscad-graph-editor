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

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// This is our main editing interface for editing graphs of invokable things (functions/modules). It is the
    /// heavyweight alternative to <see cref="LightWeightGraph"/>. In contrast to <see cref="LightWeightGraph"/>,
    /// this graph never really changes the actual graph data but just invokes refactoring events in response to
    /// user input that will do the proper refactoring actions.
    /// </summary>
    public class ScadGraphEdit : GraphEdit, IScadGraph
    {
        private ScadNode _entryPoint;
        private readonly HashSet<string> _selection = new HashSet<string>();

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
        public event Action<ScadGraphEdit, ScadNode, Vector2> NodePopupRequested;

        /// <summary>
        /// Emitted when the user requires the dialog to add a node.
        /// </summary>
        public event Action<RequestContext> AddDialogRequested;

        /// <summary>
        /// Called when data from any list entry is dropped.
        /// </summary>
        public event Action<ScadGraphEdit,ProjectTreeEntry, Vector2, Vector2> ItemDataDropped;
        
        public InvokableDescription Description { get; private set; }


        private readonly Dictionary<string, ScadNodeWidget> _widgets = new Dictionary<string, ScadNodeWidget>();
        private readonly Dictionary<string, ScadNode> _allNodes = new Dictionary<string,ScadNode>();
        private readonly List<ScadConnection> _allConnections = new List<ScadConnection>();

        private ScadConnection _pendingDisconnect;


        public IEnumerable<ScadNode> GetAllNodes()
        {
            return _allNodes.Values;
        }
        
        
        public void SelectNodes(List<ScadNode> nodes)
        {
            var set = nodes.Select(it => it.Id).ToHashSet();
            _widgets.Values.ForAll(it => it.Selected = set.Contains(it.BoundNode.Id));
            _selection.Clear();
            set.ForAll(it => _selection.Add(it));
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
                ItemDataDropped?.Invoke(this, itemListDragData.Entry, GetGlobalMousePosition(), position + ScrollOffset);
            }
        }

        private ScadNodeWidget CreateOrGet(ScadNode node)
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
                    PerformRefactorings( "Toggle literal", new ToggleLiteralRefactoring(this, node, port, enabled));
                widget.LiteralValueChanged += (port, value) =>
                    PerformRefactorings("Set literal value", new SetLiteralValueRefactoring(this, node, port, value));

                _widgets[node.Id] = widget;
                widget.MoveToNewParent(this);
            }
            
            widget.BindTo(this, node);
            return widget;
        }

        private void OnEndNodeMove()
        {
            // all nodes which are currently selected have moved, so we need to send a refactoring request for them.
            PerformRefactorings("Move node(s)",
                GetSelectedNodes()
                    .Select(it => new ChangeNodePositionRefactoring(this, it, _widgets[it.Id].Offset)));

        }
        
        public void FocusEntryPoint()
        {
            var widget = _widgets[_entryPoint.Id];
            ScrollOffset = widget.Offset - new Vector2(100, 100);
        }

        public void LoadFrom(SavedGraph graph, IReferenceResolver resolver)
        {
            Description = graph.Description;
            _allNodes.Clear();
            _allConnections.Clear();

            // first restore data to internal structures
            
            foreach (var savedNode in graph.Nodes)
            {
                var node = NodeFactory.FromSavedNode(savedNode, resolver);
                _allNodes[node.Id] = node;
                if (node is EntryPoint)
                {
                    _entryPoint = node;
                }
            }

            foreach (var connection in graph.Connections)
            {
                var scadConnection = new ScadConnection(this, _allNodes[connection.FromId], connection.FromPort,
                    _allNodes[connection.ToId], connection.ToPort);
                _allConnections.Add(scadConnection);
            }

            Name = _entryPoint.NodeTitle;

            // now rebuild the widgets and visual connections


            // rebuild the nodes
            foreach (var node in _allNodes.Values)
            {
                CreateOrGet(node); 
                
            }

            // redo the connections
            ClearConnections();
            
            foreach (var connection in _allConnections)
            {
                // connection contain ScadNode ids but we need to connect widgets, so first we need to find the 
                // the widget and then connect these widgets.
                ConnectNode(_widgets[connection.From.Id].Name, connection.FromPort, _widgets[connection.To.Id].Name, connection.ToPort);
            }
            
            // finally destroy all the widgets that are not in the graph anymore
            // set of Ids of nodes that are in the graph
            var nodeIds = graph.Nodes.Select(n => n.Id).ToHashSet();
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

        public void SaveInto(SavedGraph graph)
        {
            graph.Description = Description;

            foreach (var node in _allNodes.Values)
            {
                var savedNode = Prefabs.New<SavedNode>();
                node.SaveInto(savedNode);
                graph.Nodes.Add(savedNode);
            }

            foreach (var connection in GetAllConnections())
            {
                var savedConnection = Prefabs.New<SavedConnection>();
                savedConnection.FromId = connection.From.Id;
                savedConnection.FromPort = connection.FromPort;
                savedConnection.ToId = connection.To.Id;
                savedConnection.ToPort = connection.ToPort;
                graph.Connections.Add(savedConnection);
            }
        }

        private void OnPopupRequest(Vector2 position)
        {
            var relativePosition = position - RectGlobalPosition;
            relativePosition *= Zoom;

            var matchingWidgets = _widgets.Values
                .FirstOrDefault(it => new Rect2((it.Offset - ScrollOffset) * Zoom, it.RectSize * Zoom).HasPoint(relativePosition));

            if (matchingWidgets == null)
            {
                return;
            }

            NodePopupRequested?.Invoke(this, matchingWidgets.BoundNode, position);
        }


        public override void _GuiInput(InputEvent evt)
        {
            if (evt is InputEventMouseButton mouseButtonEvent && !mouseButtonEvent.Pressed && _pendingDisconnect != null)
            {
                GD.Print("Resolving pending disconnect.");
                PerformRefactorings("Remove connection", new DeleteConnectionRefactoring(_pendingDisconnect));
                _pendingDisconnect = null;
            }

            if (evt.IsCopy())
            {
                CopyRequested?.Invoke(this, GetSelectedNodes().ToList());
            }

            if (evt.IsCut())
            {
                CutRequested?.Invoke(this, GetSelectedNodes().ToList());
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
                    pastePosition = globalMousePosition - RectGlobalPosition + ScrollOffset;
                    
                }
                
                PasteRequested?.Invoke(this, pastePosition);
            }
        }

        private IEnumerable<ScadNode> GetSelectedNodes()
        {
            return _selection.Select(it => _widgets[it].BoundNode);
        }

        private void OnDisconnectionRequest(string fromWidgetName, int fromSlot, string toWidgetName, int toSlot)
        {
            GD.Print("Disconnect request.");
            var connection = new ScadConnection(this,ScadNodeForWidgetName(fromWidgetName), fromSlot,
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
            var context = RequestContext.From(this, ScrollOffset+releasePosition, ScadNodeForWidgetName(fromWidgetName), fromPort);
            AddDialogRequested?.Invoke(context);
        }

        private void OnConnectionFromEmpty(string toWidgetName, int toPort, Vector2 releasePosition)
        {
            
            var context = RequestContext.To(this, releasePosition, ScadNodeForWidgetName(toWidgetName), toPort);
            AddDialogRequested?.Invoke(context);
        }

        private void OnNodeSelected(ScadNodeWidget node)
        {
            _selection.Add(node.BoundNode.Id);
        }

        private void OnNodeUnselected(ScadNodeWidget node)
        {
            _selection.Remove(node.BoundNode.Id);
        }

        private void OnDeleteSelection()
        {
            var refactorings = 
                GetSelectedNodes()
                    .Select(it => new DeleteNodeRefactoring(this, it))
                    .ToList();
            _selection.Clear();
            PerformRefactorings("Delete selection",  refactorings);
        }


        private void OnConnectionRequest(string fromWidgetName, int fromPort, string toWidgetName, int toPort)
        {
            var connection = new ScadConnection(this, ScadNodeForWidgetName(fromWidgetName), fromPort,
                ScadNodeForWidgetName(toWidgetName), toPort);
            if (_pendingDisconnect != null)
            {
                if (_pendingDisconnect.RepresentsSameAs(connection))
                {
                    GD.Print("Re-connected pending node.");
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


        public IEnumerable<ScadConnection> GetAllConnections()
        {
            return _allConnections;
        }

        public void Discard()
        {
            this.RemoveAndFree();
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


        public string Render()
        {
            return _entryPoint.Render(this);
        }
    }

}