using Godot;
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
    }
}