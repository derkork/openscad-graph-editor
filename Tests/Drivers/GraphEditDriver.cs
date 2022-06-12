using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class GraphEditDriver : ControlDriver<ScadGraphEdit>
    {
        public GraphEditDriver(Func<ScadGraphEdit> producer) : base(producer)
        {
        }

        /// <summary>
        /// Checks if the graph edit has a connection from the given node to the given target node on the
        /// given ports.
        /// </summary>
        public bool HasConnection(GraphNodeDriver from, Port fromPort, GraphNodeDriver to, Port toPort)
        {
            if (!fromPort.IsOutput)
            {
                throw new ArgumentException("fromPort must be an output port");
            }

            if (!toPort.IsInput)
            {
                throw new ArgumentException("toPort must be an input port");
            }

            var graphEdit = Root;
            var fromRoot = from.Root;
            var toRoot = to.Root;
            if (graphEdit == null || fromRoot == null || toRoot == null)
            {
                return false;
            }

            return graphEdit.GetConnectionList().Cast<Dictionary>()
                .Any(connection =>
                    (string) connection["from"] == fromRoot.Name 
                    && (int) connection["from_port"] == fromPort.PortIndex 
                    && (string) connection["to"] == toRoot.Name 
                    && (int) connection["to_port"] == toPort.PortIndex);
        }

        public IEnumerable<GraphNodeDriver> Nodes
        {
            get
            {
                var root = Root;
                if (root == null)
                {
                    yield break;
                }

                var childNodes = root.GetChildNodes<GraphNode>().ToList();
                foreach (var nodePath in childNodes.Select(childNode => childNode.GetPath()))
                {
                    yield return new GraphNodeDriver(() => Root?.GetNode(nodePath) as GraphNode);
                }

            }
        }
    }
}