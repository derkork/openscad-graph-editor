using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor
{
    [UsedImplicitly]
    public class GraphEditor : Control
    {
        private GraphEdit _graphEdit;
        private AddDialog _addDialog;
        private Start _startNode;
        private TextEdit _textEdit;

        private Vector2 _lastReleasePosition;
        private ScadNode _lastSourceNode;
        private ScadNode _lastDestinationNode;
        private int _lastSlot;

        private readonly HashSet<ScadNode> _selection = new HashSet<ScadNode>();

        public override void _Ready()
        {
            _graphEdit = this.WithName<GraphEdit>("GraphEdit");
            _addDialog = this.WithName<AddDialog>("AddDialog");
            _startNode = this.WithName<Start>("Start");
            _textEdit = this.WithName<TextEdit>("TextEdit");


            _graphEdit.Connect("connection_request")
                .To(this, nameof(OnConnectionRequest));
            _graphEdit.Connect("disconnection_request")
                .To(this, nameof(OnDisconnectionRequest));
            _graphEdit.Connect("connection_to_empty")
                .To(this, nameof(OnConnectionToEmpty));
            _graphEdit.Connect("connection_from_empty")
                .To(this, nameof(OnConnectionFromEmpty));
            _graphEdit.Connect("node_selected")
                .To(this, nameof(OnNodeSelected));
            _graphEdit.Connect("node_unselected")
                .To(this, nameof(OnNodeUnselected));
            _graphEdit.Connect("delete_nodes_request")
                .To(this, nameof(OnDeleteSelection));


            _addDialog.Connect(nameof(AddDialog.NodeSelected))
                .To(this, nameof(OnNodeAdded));

            Render();
        }

        private ScadNode Named(string name)
        {
            return _graphEdit.WithName<ScadNode>(name);
        }


        private void OnConnectionRequest(string from, int fromSlot, string to, int toSlot)
        {
            DoConnect(Named(from), fromSlot, Named(to), toSlot);
            Render();
        }

        private void DoConnect(ScadNode fromNode, int formPort, ScadNode toNode, int toPort)
        {
            if (fromNode == toNode)
            {
                return; // cannot connect a node to itself.
            }

            // if the source node is not an expression node then delete all connections
            // from the source port
            if (!(fromNode is ScadExpressionNode))
            {
                _graphEdit.GetConnections()
                    .Where(it => it.IsFrom(fromNode, formPort))
                    .ForAll(DoDisconnect);
            }
            
            // also delete all connections to the target port
            _graphEdit.GetConnections()
                .Where(it => it.IsTo(toNode, toPort))
                .ForAll(DoDisconnect);

            
            _graphEdit.ConnectNode(fromNode, formPort, toNode, toPort);
            fromNode.PortConnected(formPort, false);
            toNode.PortConnected(toPort, true);
        }

        private void DoDisconnect(GraphEditExt.GraphConnection connection)
        {
            connection.Disconnect();

            // notify nodes
            connection.GetFromNode<ScadNode>().PortDisconnected(connection.FromPort, false);
            connection.GetToNode<ScadNode>().PortDisconnected(connection.ToPort, true);
        }

        private void Render()
        {
            _textEdit.Text = _startNode.Render(new ScadContext(_graphEdit));
        }

        private void OnDisconnectionRequest(string from, int fromSlot, string to, int toSlot)
        {
            DoDisconnect(new GraphEditExt.GraphConnection(_graphEdit, from, fromSlot, to, toSlot));
            Render();
        }


        private void OnConnectionToEmpty(string from, int fromSlot, Vector2 releasePosition)
        {
            _lastSourceNode = Named(from);
            _lastSlot = fromSlot;
            _lastReleasePosition = releasePosition;

            _addDialog.Open(it => it.HasInputOfType(_lastSourceNode.GetOutputPortType(fromSlot)));
        }

        private void OnConnectionFromEmpty(string to, int toSlot, Vector2 releasePosition)
        {
            _lastDestinationNode = Named(to);
            _lastSlot = toSlot;
            _lastReleasePosition = releasePosition;
            _addDialog.Open(it => it.HasOutputOfType(_lastDestinationNode.GetInputPortType(toSlot)));
        }


        private void OnNodeAdded(ScadNode node)
        {
            node.Name = Guid.NewGuid().ToString();
            node.MoveToNewParent(_graphEdit);
            node.ConnectChanged()
                .To(this, nameof(Render));
            node.Offset = _lastReleasePosition;

            if (_lastDestinationNode != null)
            {
                var index = node.GetFirstOutputPortOfType(_lastDestinationNode.GetInputPortType(_lastSlot));
                if (index > -1)
                {
                    DoConnect(node, index, _lastDestinationNode, _lastSlot);
                }
            }

            if (_lastSourceNode != null)
            {
                var index = node.GetFirstInputPortOfType(_lastSourceNode.GetOutputPortType(_lastSlot));
                if (index > -1)
                {
                    DoConnect(_lastSourceNode, _lastSlot, node, index);
                }
            }

            _lastSourceNode = null;
            _lastDestinationNode = null;
            Render();
        }

        private void OnNodeSelected(ScadNode node)
        {
            _selection.Add(node);
        }

        private void OnNodeUnselected(ScadNode node)
        {
            _selection.Remove(node);
        }

        private void OnDeleteSelection()
        {
            foreach (var node in _selection)
            {
                if (node == _startNode)
                {
                    continue; // don't allow to delete the start node
                }
                // disconnect all connections which involve the given node.
                _graphEdit.GetConnections()
                    .Where(it => it.InvolvesNode(node))
                    .ForAll(DoDisconnect);
                node.RemoveAndFree();
            }

            _selection.Clear();
            Render();
        }
    }
}