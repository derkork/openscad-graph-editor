using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public static class ScadConnectionExt
    {
        public static bool InvolvesNode(this ScadConnection connection, ScadNode node)
        {
            return connection.From == node || connection.To == node;
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
            return connection.From.OutputPortCount > connection.FromPort &&  connection.From.GetOutputPortType(connection.FromPort) == type;
        }

        public static bool IsToPortType(this ScadConnection connection, PortType type)
        {
            return connection.To.InputPortCount > connection.ToPort && connection.To.GetInputPortType(connection.ToPort) == type;
        }

        public static bool TryGetFromPortType(this ScadConnection connection, out PortType result)
        {
            if (connection.From.OutputPortCount > connection.FromPort)
            {
                result = connection.From.GetOutputPortType(connection.FromPort);
                return true;
            }

            result = default;
            return false;
        }
        
        public static bool TryGetToPortType(this ScadConnection connection, out PortType result)
        {
            if (connection.To.InputPortCount > connection.ToPort)
            {
                result = connection.To.GetInputPortType(connection.ToPort);
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