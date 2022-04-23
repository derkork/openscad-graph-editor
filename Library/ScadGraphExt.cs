using System.Linq;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public static class ScadGraphExt
    {

        public static bool IsPortConnected(this IScadGraph self, ScadNode node, PortId port)
        {
            return self.GetAllConnections().Any(it => it.InvolvesPort(node, port));
        }

        /// <summary>
        /// Tries to get the other node of a connection from or to the given port. If multiple connections exist
        /// returns an arbitrary node.
        /// </summary>
        public static bool TryGetConnectedNode(this IScadGraph self, ScadNode node, PortId port, out ScadNode result, out PortId otherPort)
        {
            var connections = self.GetAllConnections().Where(it => it.InvolvesPort(node, port));
            foreach (var connection in connections)
            {
                if (port.IsInput)
                {
                    result = connection.From;
                    otherPort = PortId.Output(connection.FromPort);
                }
                else
                {
                    result = connection.To;
                    otherPort = PortId.Input(connection.ToPort);
                }

                return true;
            }

            result = default;
            otherPort = default;
            return false;
        } 
        
        public static bool TryGetIncomingNode(this IScadGraph self, ScadNode node, int port, out ScadNode result,
            out int originatingPort)
        {
            foreach (var connection in self.GetAllConnections()
                         .Where(connection => connection.To == node && connection.ToPort == port))
            {
                result = connection.From;
                originatingPort = connection.FromPort;
                return true;
            }

            result = default;
            originatingPort = default;
            return false;
        }

        public static bool TryGetOutgoingNode(this IScadGraph self, ScadNode node, int port, out ScadNode result,
            out int targetPort)
        {
            foreach (var connection in self.GetAllConnections()
                         .Where(connection => connection.From == node && connection.FromPort == port))
            {
                result = connection.To;
                targetPort = connection.ToPort;
                return true;
            }

            result = default;
            targetPort = default;
            return false;
        }
    }
}