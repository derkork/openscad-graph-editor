using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
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
        public delegate void NeedsUpdate(bool codeChange);

        public InvokableDescription Description { get; private set; }


        private readonly Dictionary<ScadNode, ScadNodeWidget> _widgets = new Dictionary<ScadNode, ScadNodeWidget>();

        public override void _Ready()
        {
            RightDisconnects = true;

            // allow to connect "Any" nodes to anything else, except "Flow" nodes.
            Enum.GetValues(typeof(PortType))
                .Cast<int>()
                .Where(x => x != (int) PortType.Flow && x != (int) PortType.Any)
                .ForAll(x =>
                {
                    AddValidConnectionType((int) PortType.Any, x);
                    AddValidConnectionType(x, (int) PortType.Any);
                });

            // allow to connect "Reroute" nodes to anything else
            Enum.GetValues(typeof(PortType))
                .Cast<int>()
                .Where(x => x != (int) PortType.Reroute)
                .ForAll(x =>
                {
                    AddValidConnectionType((int) PortType.Reroute, x);
                    AddValidConnectionType(x, (int) PortType.Reroute);
                });


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


        private void OnDisconnectionRequest(string fromWidgetName, int fromSlot, string toWidgetName, int toSlot)
        {
            DoDisconnect(new ScadConnection(ScadNodeForWidgetName(fromWidgetName), fromSlot,
                ScadNodeForWidgetName(toWidgetName), toSlot));
            NotifyUpdateRequired(true);
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
                    it => Description.CanUse(it) &&
                          it.HasInputThatCanConnect(context.SourceNode.GetOutputPortType(fromPort)));
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
                    it => Description.CanUse(it) &&
                          it.HasOutputThatCanConnect(context.DestinationNode.GetInputPortType(toPort)));
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
            foreach (var widget in _selection)
            {
                var scadNode = widget.BoundNode;
                if (scadNode is ICannotBeDeleted)
                {
                    continue; // don't allow to delete certain nodes (e.g. the entry point and return nodes).
                }

                // disconnect all connections which involve the given node.
                GetAllConnections()
                    .Where(it => it.InvolvesNode(scadNode))
                    .ForAll(DoDisconnect);

                _widgets.Remove(scadNode);
                widget.RemoveAndFree();
            }

            _selection.Clear();
            NotifyUpdateRequired(true);
        }


        private void OnConnectionRequest(string fromWidgetName, int fromSlot, string toWidgetName, int toSlot)
        {
            DoConnect(ScadNodeForWidgetName(fromWidgetName), fromSlot, ScadNodeForWidgetName(toWidgetName), toSlot);
        }

        private void DoConnect(ScadNode fromNode, int fromPort, ScadNode toNode, int toPort)
        {
            if (fromNode == toNode)
            {
                return; // cannot connect a node to itself.
            }

            // if the source node is not an expression node then delete all connections
            // from the source port
            if (!(fromNode is ScadExpressionNode) && !(fromNode is IMultiExpressionOutputNode multiNode &&
                                                       multiNode.IsExpressionPort(fromPort)))
            {
                GetAllConnections()
                    .Where(it => it.IsFrom(fromNode, fromPort))
                    .ForAll(DoDisconnect);
            }

            // also delete all connections to the target port
            GetAllConnections()
                .Where(it => it.IsTo(toNode, toPort))
                .ForAll(DoDisconnect);

            // Reroute nodes work like this:
            // if a reroute node that is untyped is connected to a non-reroute node, the non-reroute node
            // connection type wins. 
            // if a reroute node is connected to another reroute node, the most specific
            // connection wins (e.g. is less specific than anything else).

            var fromType = fromNode.GetOutputPortType(fromPort);
            var toType = toNode.GetInputPortType(toPort);

            if (fromType == PortType.Reroute)
            {
                SwitchRerouteNodeType((RerouteNode) fromNode, toType);
            }

            if (toType == PortType.Reroute)
            {
                SwitchRerouteNodeType((RerouteNode) toNode, fromType);
            }

            ConnectNode(_widgets[fromNode].Name, fromPort, _widgets[toNode].Name, toPort);
            _widgets[fromNode].PortConnected(fromPort, false);
            _widgets[toNode].PortConnected(toPort, true);
            NotifyUpdateRequired(true);
        }

        /// <summary>
        /// Switches the reroute node to a new connection type. Cleans up all non-matching connections.
        /// </summary>
        private void SwitchRerouteNodeType(RerouteNode node, PortType newConnectionType)
        {
            var currentConnectionType = node.GetInputPortType(0);
            if (currentConnectionType == newConnectionType)
            {
                return; // nothing to do
            }

            // update the port type
            node.UpdatePortType(newConnectionType);

            // then drop all connections from or to this reroute node
            GetAllConnections()
                .Where(it => it.InvolvesNode(node))
                .ForAll(DoDisconnect);
        }

        private void DoDisconnect(ScadConnection connection)
        {
            DisconnectNode(_widgets[connection.From].Name, connection.FromPort, _widgets[connection.To].Name,
                connection.ToPort);

            if (connection.From is RerouteNode fromReroute &&
                !GetAllConnections().Any(it => it.InvolvesNode(connection.From)))
            {
                // change back to Reroute
                fromReroute.UpdatePortType(PortType.Reroute);
            }

            if (connection.To is RerouteNode toReroute &&
                !GetAllConnections().Any(it => it.InvolvesNode(connection.To)))
            {
                // change back to Reroute
                toReroute.UpdatePortType(PortType.Reroute);
            }


            // notify nodes
            _widgets[connection.From].PortDisconnected(connection.FromPort, false);
            _widgets[connection.To].PortDisconnected(connection.ToPort, true);
        }

        private ScadNode ScadNodeForWidgetName(string widgetName)
        {
            return this.AtPath<ScadNodeWidget>(widgetName).BoundNode;
        }


        private void OnNodeAdded(ScadNode node, NodeAddContext context)
        {
            node.Offset = context.LastReleasePosition + ScrollOffset;
            CreateWidgetFor(node);

            if (context.DestinationNode != null)
            {
                var index = node.GetFirstOutputThatCanConnect(context.DestinationNode.GetInputPortType(context.LastPort));
                if (index > -1)
                {
                    DoConnect(node, index, context.DestinationNode, context.LastPort);
                }
            }

            if (context.SourceNode != null)
            {
                var index = node.GetFirstInputThatCanConnect(context.SourceNode.GetOutputPortType(context.LastPort));
                if (index > -1)
                {
                    DoConnect(context.SourceNode, context.LastPort, node, index);
                }
            }

            NotifyUpdateRequired(true);
        }


        public IEnumerable<ScadConnection> GetAllConnections()
        {
            return GetConnectionList()
                .Cast<Godot.Collections.Dictionary>()
                .Select(item => new ScadConnection(
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