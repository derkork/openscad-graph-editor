using System.Linq;
using Godot;
using Godot.Collections;
using GodotExt;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public readonly struct ScadContext
    {
        private readonly GraphEdit _graphEdit;

        public ScadContext(GraphEdit graphEdit)
        {
            _graphEdit = graphEdit;
        }


        public bool TryGetInputNode(ScadNode node, int inputPort, out ScadNode connected)
        {
            if (_graphEdit.TryGetFirst(it => it.IsTo(node, inputPort), out var connection))
            {
                connected = connection.GetFromNode<ScadNode>();
                return true;
            }

            connected = default;
            return false;
        }

        public bool TryGetOutputNode(ScadNode node, int inputPort, out ScadNode connected)
        {
            if (_graphEdit.TryGetFirst(it => it.IsFrom(node, inputPort), out var connection))
            {
                connected = connection.GetToNode<ScadNode>();
                return true;
            }

            connected = default;
            return false;
        }

        public void Load(SavedGraph graph)
        {
            _graphEdit.ClearConnections();
            _graphEdit.GetChildNodes<ScadNode>().ForAll(it => it.RemoveAndFree());

            foreach (var savedNode in graph.Nodes)
            {
                var instance = NodeFactory.FromScript(savedNode.Script);
                instance.PrepareForLoad(savedNode);
                instance.MoveToNewParent(_graphEdit);
                instance.LoadFrom(savedNode);
            }

            foreach (var savedConnection in graph.Connections)
            {
                _graphEdit.ConnectNode(savedConnection.FromId, savedConnection.FromPort, savedConnection.ToId,
                    savedConnection.ToPort);
            }
        }

        public SavedGraph Save()
        {
            var result = Prefabs.New<SavedGraph>();
            result.Nodes = new Array<SavedNode>(
                _graphEdit
                    .GetChildNodes<ScadNode>()
                    .Select(it =>
                    {
                        var savedNode = Prefabs.New<SavedNode>();
                        it.SaveInto(savedNode);
                        return savedNode;
                    })
            );

            result.Connections = new Array<SavedConnection>(
                _graphEdit
                    .GetConnections()
                    .Select(it =>
                    {
                        var savedConnection = Prefabs.New<SavedConnection>();
                        savedConnection.FromId = it.From;
                        savedConnection.FromPort = it.FromPort;
                        savedConnection.ToId = it.To;
                        savedConnection.ToPort = it.ToPort;
                        return savedConnection;
                    })
            );

            return result;
        }
    }
}