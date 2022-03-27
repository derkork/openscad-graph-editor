using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// This is our main editing interface for editing graphs of invokable things (functions/modules). It is the
    /// heavyweight alternative to <see cref="LightWeightGraph"/>
    /// </summary>
    public class ScadGraphEdit : GraphEdit, IScadGraph
    {
        private ScadNode _entryPoint;
        private readonly HashSet<ScadNodeWidget> _selection = new HashSet<ScadNodeWidget>();
        private AddDialog.AddDialog _addDialog;
        private RefactoringPopup _refactoringPopup;
        
        [Signal]
        public delegate void RequestRefactorings(Refactoring[] refactorings);

        [Signal]
        public delegate void NeedsUpdate(bool codeChange);

        public InvokableDescription Description { get; private set; }


        private readonly Dictionary<ScadNode, ScadNodeWidget> _widgets = new Dictionary<ScadNode, ScadNodeWidget>();

        private ScadConnection _pendingDisconnect = null;

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
        }

        public override bool CanDropData(Vector2 position, object data)
        {
            if (!(data is Reference reference))
            {
                return false;
            }

            return reference.TryGetBeer(out DragData[] _);
        }

        public override void DropData(Vector2 position, object data)
        {

            if (!(data is Reference reference) || !reference.TryGetBeer(out DragData[] dragData))
            {
                return;
            }

            if (dragData.Length == 1)
            {
                OnNodeAdded(dragData[0].Data(), NodeAddContext.AtPosition(position));
            }
        }

        private void CreateWidgetFor(ScadNode node)
        {
            var widget = node is RerouteNode
                ? Prefabs.InstantiateFromScene<RerouteNodeWidget.RerouteNodeWidget>()
                : Prefabs.New<ScadNodeWidget>();

            widget.ConnectChanged()
                .To(this, nameof(NotifyUpdateRequired));

            _widgets[node] = widget;
            widget.MoveToNewParent(this);
            widget.BindTo(node);
        }


        public void Setup(AddDialog.AddDialog addDialog, RefactoringPopup refactoringPopup)
        {
            _addDialog = addDialog;
            _refactoringPopup = refactoringPopup;
        }

        public void FocusEntryPoint()
        {
            var widget = _widgets[_entryPoint];
            ScrollOffset = widget.Offset - new Vector2(100, 100);
        }

        public void LoadFrom(SavedGraph graph, IReferenceResolver resolver)
        {
            Clear();

            Description = graph.Description;

            foreach (var savedNode in graph.Nodes)
            {
                var node = NodeFactory.FromSavedNode(savedNode, resolver);
                CreateWidgetFor(node);

                if (node is EntryPoint)
                {
                    _entryPoint = node;
                }
            }

            foreach (var connection in graph.Connections)
            {
                // connection contain ScadNode ids but we need to connect widgets, so first we need to find the 
                // node for the given IDs and then the widget
                var fromNode = _widgets.Keys.First(it => it.Id == connection.FromId);
                var toNode = _widgets.Keys.First(it => it.Id == connection.ToId);

                ConnectNode(_widgets[fromNode].Name, connection.FromPort, _widgets[toNode].Name, connection.ToPort);
                // restore literal visibility
                _widgets[fromNode].PortConnected(connection.FromPort, false);
                _widgets[toNode].PortConnected(connection.ToPort, true);
            }
        }

        public void SaveInto(SavedGraph graph)
        {
            graph.Description = Description;

            foreach (var node in _widgets.Keys)
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
                .Where(it =>
                    new Rect2((it.Offset - ScrollOffset) * Zoom, it.RectSize * Zoom).HasPoint(relativePosition));

            foreach (var widget in matchingWidgets)
            {
                _refactoringPopup.Open(position, this, widget.BoundNode);
                break;
            }
        }


        public override void _GuiInput(InputEvent evt)
        {
            if (evt is InputEventMouseButton mouseButtonEvent && !mouseButtonEvent.Pressed && _pendingDisconnect != null)
            {
                GD.Print("Resolving pending disconnect.");
                if (DisconnectWithChecks(_pendingDisconnect, out var refactorings))
                {
                    PerformRefactorings(refactorings);
                }
                else
                {
                    // was vetoed, so restore the visible connection
                    ConnectNode(_widgets[_pendingDisconnect.From].Name, _pendingDisconnect.FromPort, 
                        _widgets[_pendingDisconnect.To].Name, _pendingDisconnect.ToPort);
                }
                _pendingDisconnect = null;
            }
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
            var context = NodeAddContext.From(releasePosition, ScadNodeForWidgetName(fromWidgetName), fromPort);

            if (Input.IsKeyPressed((int) KeyList.Shift))
            {
                OnNodeAdded(NodeFactory.Build<RerouteNode>(), context);
            }
            else
            {
                _addDialog.Open(it => OnNodeAdded(it, context),
                    it => Description.CanUse(it) && CanAcceptConnectionFrom(context.SourceNode, fromPort, it, out _));
            }
        }

        private void OnConnectionFromEmpty(string toWidgetName, int toPort, Vector2 releasePosition)
        {
            
            var context = NodeAddContext.To(releasePosition, ScadNodeForWidgetName(toWidgetName), toPort);
            if (Input.IsKeyPressed((int) KeyList.Shift))
            {
                OnNodeAdded(NodeFactory.Build<RerouteNode>(), context);
            }
            else
            {
                _addDialog.Open(it => OnNodeAdded(it, context),
                    it => Description.CanUse(it) && CanAcceptConnectionTo(context.DestinationNode, toPort, it, out _));
            }
        }

        private void OnNodeSelected(ScadNodeWidget node)
        {
            _selection.Add(node);
        }

        private void OnNodeUnselected(ScadNodeWidget node)
        {
            _selection.Remove(node);
        }

        private void OnDeleteSelection()
        {
            var refactorings = new List<Refactoring>();

            foreach (var widget in _selection)
            {
                var scadNode = widget.BoundNode;
                if (scadNode is ICannotBeDeleted)
                {
                    continue; // don't allow to delete certain nodes (e.g. the entry point and return nodes).
                }


                var refactoringsForNode = new List<Refactoring>();
                var connectionsToDrop = GetAllConnections().Where(it => it.InvolvesNode(scadNode));
                var nodeVetoed = false;
                foreach (var connection in connectionsToDrop)
                {
                    if (DisconnectWithChecks(connection, out var dropConnectionRefactorings))
                    {
                        refactoringsForNode.AddRange(dropConnectionRefactorings);
                    }
                    else
                    {
                        // some disconnect was vetoed, so we can't delete this node.
                        nodeVetoed = true;
                        break;
                    }
                }
                
                // if node was vetoed, continue with the next node.
                if (nodeVetoed)
                {
                    continue;
                }
                
                refactoringsForNode.Add(new DeleteNodeRefactoring(this, scadNode));
                refactorings.AddRange(refactoringsForNode);
            }

            _selection.Clear();
            PerformRefactorings(refactorings);
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
            
            
            var refactorings = ConnectWithChecks(connection);
            PerformRefactorings(refactorings);
        }

        [MustUseReturnValue]
        private IEnumerable<Refactoring> ConnectWithChecks(ScadConnection connection)
        {
            var result = ConnectionRules.CanConnect(connection);

            if (result.Decision == ConnectionRules.OperationRuleDecision.Veto)
            {
                GD.Print("Connection vetoed.");
                return Enumerable.Empty<Refactoring>();
            }

            var createConnectionRefactoring = new CreateConnectionRefactoring(connection);
            return result.Refactorings.Append(createConnectionRefactoring);
        }


        [MustUseReturnValue]
        private bool DisconnectWithChecks(ScadConnection connection, out IEnumerable<Refactoring> refactorings)
        {
            var result = ConnectionRules.CanDisconnect(connection);
            
            if (result.Decision == ConnectionRules.OperationRuleDecision.Veto)
            {
                GD.Print("Disconnection vetoed.");
                refactorings = Enumerable.Empty<Refactoring>();
                return false;
            }

            refactorings = result.Refactorings.Append(new DropConnectionRefactoring(connection));
            return true;
        }

        private ScadNode ScadNodeForWidgetName(string widgetName)
        {
            return this.AtPath<ScadNodeWidget>(widgetName).BoundNode;
        }


        private void OnNodeAdded(ScadNode node, NodeAddContext context)
        {
            node.Offset = context.LastReleasePosition + ScrollOffset;

            var refactorings = new List<Refactoring> {new AddNodeRefactoring(this, node)};

            if (context.DestinationNode != null)
            {
                if (CanAcceptConnectionTo(context.DestinationNode, context.LastPort, node, out var fromPort))
                {
                    refactorings.AddRange(ConnectWithChecks(new ScadConnection(this, node, fromPort, context.DestinationNode, context.LastPort)));
                }
            }

            if (context.SourceNode != null)
            {
                if (CanAcceptConnectionFrom(context.SourceNode, context.LastPort, node, out var toPort))
                {
                    refactorings.AddRange(ConnectWithChecks(new ScadConnection(this, context.SourceNode, context.LastPort, node, toPort)));
                }
            }

            PerformRefactorings(refactorings);
        }


        private bool CanAcceptConnectionFrom(ScadNode from, int fromPort, ScadNode to, out int toPort)
        {
            for (var i = 0; i < to.InputPortCount; i++)
            {
                var connection = new ScadConnection(this, from, fromPort, to, i);
                if (ConnectionRules.CanConnect(connection).Decision == ConnectionRules.OperationRuleDecision.Allow)
                {
                    toPort = i;
                    return true;
                }
            }

            toPort = -1;
            return false;
        }

        private bool CanAcceptConnectionTo(ScadNode to, int toPort, ScadNode from, out int fromPort)
        {
            for (var i = 0; i < from.OutputPortCount; i++)
            {
                var connection = new ScadConnection(this, from, i, to, toPort);
                if (ConnectionRules.CanConnect(connection).Decision == ConnectionRules.OperationRuleDecision.Allow)
                {
                    fromPort = i;
                    return true;
                }
            }

            fromPort = -1;
            return false;
        }
        
        
        
        public IEnumerable<ScadConnection> GetAllConnections()
        {
            return GetConnectionList()
                .Cast<Godot.Collections.Dictionary>()
                .Select(item => new ScadConnection(
                    this,
                    ScadNodeForWidgetName((string) item["from"]),
                    (int) item["from_port"],
                    ScadNodeForWidgetName((string) item["to"]),
                    (int) item["to_port"]
                ));
        }

        public void Discard()
        {
            this.RemoveAndFree();
        }

        private void Clear()
        {
            _entryPoint = null;
            _selection.Clear();
            ClearConnections();
            _widgets.Values.ForAll(it => it.RemoveAndFree());
            _widgets.Clear();
        }

        private void PerformRefactorings(IEnumerable<Refactoring> refactorings)
        {
            var refactoringsAsArray = refactorings.ToArray();
            if (refactoringsAsArray.Length == 0)
            {
                return;
            }
            EmitSignal(nameof(RequestRefactorings), new object[] { refactoringsAsArray });
        }

        private void NotifyUpdateRequired(bool codeChange)
        {
            EmitSignal(nameof(NeedsUpdate), codeChange);
        }


        public string Render()
        {
            return _entryPoint.Render(this);
        }

        private class NodeAddContext
        {

            public static NodeAddContext AtPosition(Vector2 position)
            {
                return new NodeAddContext(position, null, null, 0);
            }

            public static NodeAddContext From(Vector2 position, ScadNode node, int port)
            {
                return new NodeAddContext(position, node, null, port);
            }
            public static NodeAddContext To(Vector2 position, ScadNode node, int port)
            {
                return new NodeAddContext(position, null, node, port);
            }

            private NodeAddContext(Vector2 lastReleasePosition, ScadNode sourceNode, ScadNode destinationNode,
                int lastPort)
            {
                LastReleasePosition = lastReleasePosition;
                SourceNode = sourceNode;
                DestinationNode = destinationNode;
                LastPort = lastPort;
            }

            public Vector2 LastReleasePosition { get; }
            public ScadNode SourceNode { get; }
            public ScadNode DestinationNode { get; }
            public int LastPort { get; }
        }
    }
}