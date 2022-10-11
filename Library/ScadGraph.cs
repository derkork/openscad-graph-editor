using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// A lightweight graph representation, that can simply render to OpenScad but is not user-editable and has
    /// no visible UI.
    /// </summary>
    public class ScadGraph
    {
        private readonly List<ScadConnection> _connections = new List<ScadConnection>();
        private readonly List<ScadNode> _nodes = new List<ScadNode>();

        public InvokableDescription Description { get; private set; }

        public string Render()
        {
            // we render all nodes which have either no output at all or a single disconnected flow output
            var candidates = _nodes.Where(it =>
            {
                // helper nodes will not be used as render target.
                if (it is Comment || it is RerouteNode)
                {
                    return false;
                }
                
                if (it.OutputPortCount == 0)
                {
                    return true;
                }

                var hasFlowOutput = false;
                for (var i = 0; i < it.OutputPortCount; i++)
                {
                    var portId = PortId.Output(i);
                    if (it.GetPortType(portId) == PortType.Geometry)
                    {
                        hasFlowOutput = true;
                        if (IsPortConnected(it, portId))
                        {
                            return false;
                        }
                    }
                }

                return hasFlowOutput;
            });
            
            // sort the candidates by their position in the graph
            // nodes with a lower y value will be rendered first
            // nodes with the same y value will be sorted by their x value
            var sortedCandidates = candidates
                .OrderBy(it => it.Offset.y)
                .ThenBy(it => it.Offset.x)
                .ToList();
            
            var content = sortedCandidates.Select(it =>
            {
                var nodeContent = it.Render(this, 0);
                
                var renderModifier = it.BuildRenderModifier();
                return !renderModifier.Empty() ? string.Format(renderModifier, nodeContent) : nodeContent;
            }).JoinToString("\n");
            // now check if the graph has an entrypoint if, so render the entry point with the given content
            if (_nodes.FirstOrDefault(it => it is EntryPoint) is EntryPoint entryPoint)
            {
                content = entryPoint.RenderEntryPoint( content);
            }

            return content;
        }

        public IEnumerable<ScadNode> GetAllNodes()
        {
            return _nodes.ToList();
        }

        public void Main()
        {
            Clear();
            Description = new MainModuleDescription();
        }

        public ScadNode ById(string id)
        {
            return _nodes.First(it => it.Id == id);
        }

        public bool TryById(string id, out ScadNode node)
        {
            var result = _nodes.FirstOrDefault(it => it.Id == id);
            node = result;
            return result != null;
        }

        public void NewFromDescription(InvokableDescription description)
        {
            Clear();
            Description = description;

            switch (description)
            {
                case FunctionDescription functionDescription:
                    var entryPoint = NodeFactory.Build<FunctionEntryPoint>(functionDescription);
                    var returnNode = NodeFactory.Build<FunctionReturn>(functionDescription);
                    // don't let the nodes overlap
                    returnNode.Offset = entryPoint.Offset + new Vector2(800, 0);

                    _nodes.Add(entryPoint);
                    _nodes.Add(returnNode);
                    break;
                case ModuleDescription moduleDescription:
                    entryPoint = NodeFactory.Build<ModuleEntryPoint>(moduleDescription);
                    _nodes.Add(entryPoint);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Clear()
        {
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

        public bool IsPortConnected(ScadNode node, PortId port)
        {
            return GetAllConnections().Any(it => it.InvolvesPort(node, port));
        }
    }
}