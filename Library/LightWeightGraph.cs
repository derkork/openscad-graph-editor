using System;
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
        private ScadNode _entryPoint;
        private readonly List<ScadConnection> _connections = new List<ScadConnection>();
        private readonly List<ScadNode> _nodes = new List<ScadNode>();

        public InvokableDescription Description { get; private set; }

        public string Render()
        {
            return _entryPoint.Render(this);
        }

        public void Main()
        {
            Clear();
            Description = Prefabs.New<MainModuleDescription>();
            _entryPoint = NodeFactory.Build<MainEntryPoint>();
            _nodes.Add(_entryPoint);
        }

        public void NewFromDescription(InvokableDescription description)
        {
            Clear();
            Description = description;

            switch (description)
            {
                case FunctionDescription functionDescription:
                    _entryPoint = NodeFactory.Build<FunctionEntryPoint>(functionDescription);
                    var returnNode = NodeFactory.Build<FunctionReturn>(functionDescription);

                    _nodes.Add(_entryPoint);
                    _nodes.Add(returnNode);
                    _connections.Add(new ScadConnection(_entryPoint, 0, returnNode, 0));

                    break;
                case ModuleDescription moduleDescription:
                default:
                    throw new NotImplementedException();
            }
        }

        private void Clear()
        {
            _entryPoint = null;
            _connections.Clear();
            _nodes.Clear();
        }

        private ScadNode Lookup(string id)
        {
            return _nodes.First(it => it.Id == id);
        }

        public void LoadFrom(SavedGraph graph, IReferenceResolver resolver)
        {
            Clear();

            Description = graph.Description;

            foreach (var savedNode in graph.Nodes)
            {
                var node = NodeFactory.FromSavedNode(savedNode, resolver);
                _nodes.Add(node);

                if (node is EntryPoint)
                {
                    _entryPoint = node;
                }
            }

            foreach (var connection in graph.Connections)
            {
                var target = Lookup(connection.ToId);
                _connections.Add(new ScadConnection(Lookup(connection.FromId), connection.FromPort, target,
                    connection.ToPort));
            }
        }

        public void SaveInto(SavedGraph graph)
        {
            graph.Description = Description;

            foreach (var node in (IEnumerable<ScadNode>) _nodes)
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

        public IEnumerable<IScadConnection> GetAllConnections()
        {
            return _connections;
        }

        public void Discard()
        {
        }
    }
}