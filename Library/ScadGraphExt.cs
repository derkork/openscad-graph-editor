using System.Linq;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public static class ScadGraphExt
    {
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