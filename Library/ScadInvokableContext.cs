using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// This represents an Invokable (function or module). It can be rendered to OpenScad code.
    /// </summary>
    public class ScadInvokableContext : ICanBeRendered
    {
        private IScadGraph _graph;

        public ScadInvokableContext(IScadGraph graph)
        {
            _graph = graph;
        }

        public string Render()
        {
            return _graph.GetEntrypoint().Render(this);
        }

        public void TransferTo(IScadGraph graph)
        {
            var savedGraph = Prefabs.New<SavedGraph>();
            _graph.SaveInto(savedGraph);
            graph.LoadFrom(savedGraph, this);
            _graph.Discard();
            _graph = graph;
        }

        public bool TryGetInputNode(ScadNode node, int inputPort, out ScadNode connected)
        {
            return _graph.TryGetIncomingNode(node, inputPort, out connected);
        }

        public bool TryGetOutputNode(ScadNode node, int inputPort, out ScadNode connected)
        {
            return _graph.TryGetOutgoingNode(node, inputPort, out connected);
        }

        public void Load(SavedGraph graph)
        {
            _graph.LoadFrom(graph, this);
        }

        public SavedGraph Save()
        {
            var result = Prefabs.New<SavedGraph>();
            _graph.SaveInto(result);
            return result;
        }

        public void Discard()
        {
            _graph.Discard();
        }
    }
}