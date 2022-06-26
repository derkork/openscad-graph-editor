using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExt;
using GodotTestDriver.Drivers;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Tests.Drivers
{
    /// <summary>
    /// Driver for a <see cref="GraphEdit"/>.  
    /// </summary>
    [PublicAPI]
    public class GraphEditDriver<TGraphEdit,TGraphNodeDriver, TGraphNode> : ControlDriver<TGraphEdit> 
        where TGraphEdit:GraphEdit where TGraphNode:GraphNode where TGraphNodeDriver:GraphNodeDriver<TGraphNode>
    {
        private readonly Func<Func<TGraphNode>, string, TGraphNodeDriver> _nodeDriverProducer;

        /// <summary>
        /// Constructs a new driver.
        /// </summary>
        /// <param name="producer">a producer that produces the <see cref="GraphEdit"/> that this driver works on.</param>
        /// <param name="nodeDriverProducer">a producer that produces a driver for a <see cref="GraphNode"/> child of the <see cref="GraphEdit"/></param>
        /// <param name="description">a description for the node</param>
        public GraphEditDriver(Func<TGraphEdit> producer, 
            Func<Func<TGraphNode>,string,TGraphNodeDriver> nodeDriverProducer,
            string description = "") : base(producer, description)
        {
            _nodeDriverProducer = nodeDriverProducer;
        }

        /// <summary>
        /// Checks if the graph edit has a connection from the given node to the given target node on the
        /// given ports.
        /// </summary>
        public bool HasConnection(TGraphNodeDriver from, Port fromPort, TGraphNodeDriver to, Port toPort)
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

        public IEnumerable<TGraphNodeDriver> Nodes =>
            BuildDrivers(root => root.GetChildNodes<TGraphNode>(),
                node => _nodeDriverProducer(node, "-> GraphNode")
            );
    }
    
    /// <summary>
    /// Driver for a <see cref="GraphEdit"/>.
    /// </summary>
    [PublicAPI]
    public class GraphEditDriver : GraphEditDriver<GraphEdit, GraphNodeDriver, GraphNode>
    {
        public GraphEditDriver(Func<GraphEdit> producer, string description = "") : base(producer,
            (node, nodeDescription) => new GraphNodeDriver(node,$"{description}-> {nodeDescription}"),
            description)
        {
        }
    }
}