using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// A lightweight graph representation, that can simply render to OpenScad but is not user-editable and has
    /// no visible UI.
    /// </summary>
    public class LightWeightGraph : IScadGraph
    {
        private string _name = "";
        private ScadNode _entryPoint;
        private readonly List<ScadConnection> _connections = new List<ScadConnection>();
        private readonly List<ScadNode> _nodes = new List<ScadNode>();

        public string InvokableName => _name;

        public void Blank(string name, ScadNode entryPoint)
        {
            Clear();
            _name = name;
            _entryPoint = entryPoint;
            _nodes.Add(entryPoint);
        }

        private void Clear()
        {
            _name = "";
            _entryPoint = null;
            _connections.Clear();
            _nodes.Clear();
        }

        private ScadNode Lookup(string id)
        {
            return _nodes.First(it => it.Id == id);
        }

        public void LoadFrom(SavedGraph graph, ScadInvokableContext context)
        {
            Clear();

            _name = graph.Name;
            
            foreach (var savedNode in graph.Nodes)
            {
                var node = NodeFactory.FromType(savedNode.Type);
                node.LoadFrom(savedNode);
                _nodes.Add(node);

                if (node is IGraphEntryPoint)
                {
                    _entryPoint = node;
                }
            }

            foreach (var connection in graph.Connections)
            {
                ScadNode target = Lookup(connection.ToId);
                _connections.Add(new ScadConnection(Lookup(connection.FromId), connection.FromPort, target, connection.ToPort));
            }
        }

        public void SaveInto(SavedGraph graph)
        {
            foreach (var node in GetAllNodes())
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

        public ScadNode GetEntrypoint()
        {
            return _entryPoint;
        }

        public IEnumerable<ScadNode> GetAllNodes()
        {
            return _nodes;
        }

        public IEnumerable<IScadConnection> GetAllConnections()
        {
            return _connections;
        }

        public void Discard()
        {
        }
    }

    // TODO merge with ScadGrapEdit's ScadConnection
    internal class ScadConnection : IScadConnection
    {
        public ScadNode From { get; }
        public int FromPort { get; }
        public ScadNode To { get; }
        public int ToPort { get; }

        public ScadConnection(ScadNode from, int fromPort, ScadNode to, int toPort)
        {
            From = from;
            FromPort = fromPort;
            To = to;
            ToPort = toPort;
        }
    }
}