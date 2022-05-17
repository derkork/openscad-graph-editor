using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library.IO;
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

        public IEnumerable<ScadNode> GetAllNodes()
        {
            return _nodes.ToList();
        }

        public void Main()
        {
            Clear();
            Description = new MainModuleDescription();
            _entryPoint = NodeFactory.Build<MainEntryPoint>();
            _nodes.Add(_entryPoint);
        }

        public ScadNode ById(string id)
        {
            return _nodes.First(it => it.Id == id);
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
                    // don't let the nodes overlap
                    returnNode.Offset = _entryPoint.Offset + new Vector2(800, 0);

                    _nodes.Add(_entryPoint);
                    _nodes.Add(returnNode);
                    _connections.Add(new ScadConnection(this, _entryPoint, 0, returnNode, 0));
                    break;
                case ModuleDescription moduleDescription:
                    _entryPoint = NodeFactory.Build<ModuleEntryPoint>(moduleDescription);
                    _nodes.Add(_entryPoint);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        public void LoadFrom(SavedGraph graph, InvokableDescription description, IReferenceResolver resolver)
        {
            Clear();

            Description = description;

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
                _connections.Add(new ScadConnection(this, Lookup(connection.FromId), connection.FromPort, target,
                    connection.ToPort));
            }
        }

        public void SaveInto(SavedGraph graph)
        {
            graph.Description = Description.ToSavedState();

            foreach (var node in (IEnumerable<ScadNode>) _nodes)
            {
                graph.Nodes.Add(node.ToSavedState());
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

        public IEnumerable<ScadConnection> GetAllConnections()
        {
            return _connections;
        }

        public void Discard()
        {
        }

        public void RemoveConnection(ScadConnection connection)
        {
            var removed = _connections.Remove(connection);
            GdAssert.That(removed, "Tried to remove non-existing connection.");
        }

        public void AddConnection(string fromId, int fromPort, string toId, int toPort)
        {
            _connections.Add(new ScadConnection(this, ById(fromId), fromPort, ById(toId), toPort));
        }

        public void RemoveNode(ScadNode node)
        {
            GdAssert.That(_nodes.Contains(node), "Tried to remove non-existing node.");
            GdAssert.That(!_connections.Any(it => it.InvolvesNode(node)), "Tried to remove node with connections.");
            _nodes.Remove(node);
        }

        public void AddNode(ScadNode node)
        {
            GdAssert.That(_nodes.All(it => it.Id != node.Id), "Tried to add node which already exists.");
            _nodes.Add(node);
        }
    }
}