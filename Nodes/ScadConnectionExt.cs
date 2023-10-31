using System.Collections.Generic;
using System.Linq;

namespace OpenScadGraphEditor.Nodes
{
    public static class ScadConnectionExt
    {
        public static bool InvolvesNode(this ScadConnection connection, ScadNode node)
        {
            return connection.From == node || connection.To == node;
        }

        public static bool InvolvesAnyNode(this ScadConnection connection, IEnumerable<ScadNode> nodes)
        {
            return nodes.Any(connection.InvolvesNode);
        }

        public static bool InvolvesPort(this ScadConnection connection, ScadNode node, PortId port)
        {
            if (port.IsInput)
            {
                return connection.To == node && connection.ToPort == port.Port;
            }

            return connection.From == node && connection.FromPort == port.Port;
        }

        public static bool InvolvesAnyPort(this ScadConnection connection, ScadNode node, IEnumerable<PortId> ports)
        {
            return ports.Any(port => connection.InvolvesPort(node, port));
        }
        

        public static bool IsFrom(this ScadConnection connection, ScadNode node, int port)
        {
            return connection.From == node && connection.FromPort == port;
        }

        public static bool IsTo(this ScadConnection connection, ScadNode node, int port)
        {
            return connection.To == node && connection.ToPort == port;
        }

        public static bool IsFromPortType(this ScadConnection connection, PortType type)
        {
            return connection.From.OutputPortCount > connection.FromPort &&  connection.From.GetPortType(PortId.Output(connection.FromPort)) == type;
        }

        public static bool IsToPortType(this ScadConnection connection, PortType type)
        {
            return connection.To.InputPortCount > connection.ToPort && connection.To.GetPortType(PortId.Input(connection.ToPort)) == type;
        }

        public static bool TryGetFromPortType(this ScadConnection connection, out PortType result)
        {
            if (connection.From.OutputPortCount > connection.FromPort)
            {
                result = connection.From.GetPortType(PortId.Output(connection.FromPort));
                return true;
            }

            result = default;
            return false;
        }
        
        public static bool TryGetToPortType(this ScadConnection connection, out PortType result)
        {
            if (connection.To.InputPortCount > connection.ToPort)
            {
                result = connection.To.GetPortType(PortId.Input(connection.ToPort));
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Checks whether this connection represents the same as the other connection. The check is performed
        /// on the raw data levels, so this will work across different graph representations of the same graph.
        /// </summary>
        public static bool RepresentsSameAs(this ScadConnection self, ScadConnection other)
        {
            return self.From.Id == other.From.Id && self.FromPort == other.FromPort &&
                   self.To.Id == other.To.Id && self.ToPort == other.ToPort;
        }
        
    }
}