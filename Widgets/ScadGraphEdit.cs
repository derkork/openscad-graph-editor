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

        [Signal]
        public delegate void NeedsUpdate();

        private Vector2 _lastReleasePosition;
        private ScadNode _lastSourceNode;
        private ScadNode _lastDestinationNode;
        private int _lastPort;

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
            _lastSourceNode = null;
            _lastDestinationNode = null;
            
            if (!(data is Reference reference) || !reference.TryGetBeer(out DragData[] dragData))
            {
                return;
            }

            _lastReleasePosition = position;

            if (dragData.Length == 1)
            {
                switch (dragData[0].Data)
                {
                    case FunctionDescription functionDescription:
                        OnNodeAdded(NodeFactory.Build<FunctionInvocation>(functionDescription));
                        break;
                    case ModuleDescription moduleDescription:
                        OnNodeAdded(NodeFactory.Build<ModuleInvocation>(moduleDescription));
                        break;
                }
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


        public void Setup(AddDialog.AddDialog addDialog)
        {
            _addDialog = addDialog;
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


        private void OnDisconnectionRequest(string fromWidgetName, int fromSlot, string toWidgetName, int toSlot)
        {
            DoDisconnect(new ScadConnection(ScadNodeForWidgetName(fromWidgetName), fromSlot,
                ScadNodeForWidgetName(toWidgetName), toSlot));
            NotifyUpdateRequired();
        }


        private void OnConnectionToEmpty(string fromWidgetName, int fromPort, Vector2 releasePosition)
        {
            _lastSourceNode = ScadNodeForWidgetName(fromWidgetName);
            _lastDestinationNode = null;
            _lastPort = fromPort;
            _lastReleasePosition = releasePosition;

            if (Input.IsKeyPressed((int) KeyList.Shift))
            {
                OnNodeAdded(NodeFactory.Build<RerouteNode>());
            }
            else
            {
                _addDialog.Open(OnNodeAdded, it =>  Description.CanUse(it) && it.HasInputThatCanConnect(_lastSourceNode.GetOutputPortType(fromPort)));
            }
        }

        private void OnConnectionFromEmpty(string toWidgetName, int toPort, Vector2 releasePosition)
        {
            _lastSourceNode = null;
            _lastDestinationNode = ScadNodeForWidgetName(toWidgetName);
            _lastPort = toPort;
            _lastReleasePosition = releasePosition;
            _addDialog.Open(OnNodeAdded,
                it => Description.CanUse(it) && it.HasOutputThatCanConnect(_lastDestinationNode.GetInputPortType(toPort)));
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
            NotifyUpdateRequired();
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
            if (!(fromNode is ScadExpressionNode) && !(fromNode is IMultiExpressionOutputNode multiNode && multiNode.IsExpressionPort(fromPort)))
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
            NotifyUpdateRequired();
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
            DisconnectNode(_widgets[connection.From].Name, connection.FromPort, _widgets[connection.To].Name, connection.ToPort);

            if (connection.From is RerouteNode fromReroute && !GetAllConnections().Any(it => it.InvolvesNode(connection.From)))
            {
                // change back to Reroute
                fromReroute.UpdatePortType(PortType.Reroute);
            }

            if (connection.To is RerouteNode toReroute && !GetAllConnections().Any(it => it.InvolvesNode(connection.To)))
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


        private void OnNodeAdded(ScadNode node)
        {
            node.Offset = _lastReleasePosition + ScrollOffset;
            CreateWidgetFor(node);

            if (_lastDestinationNode != null)
            {
                var index = node.GetFirstOutputThatCanConnect(_lastDestinationNode.GetInputPortType(_lastPort));
                if (index > -1)
                {
                    DoConnect(node, index, _lastDestinationNode, _lastPort);
                }
            }

            if (_lastSourceNode != null)
            {
                var index = node.GetFirstInputThatCanConnect(_lastSourceNode.GetOutputPortType(_lastPort));
                if (index > -1)
                {
                    DoConnect(_lastSourceNode, _lastPort, node, index);
                }
            }

            NotifyUpdateRequired();
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
            ClearConnections();
            _widgets.Values.ForAll(it => it.RemoveAndFree());
            _widgets.Clear();
        }

        private void NotifyUpdateRequired()
        {
            EmitSignal(nameof(NeedsUpdate));
        }
        

        public string Render()
        {
            return _entryPoint.Render(this);
        }
    }
}