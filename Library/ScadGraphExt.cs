using System.Linq;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public static class ScadGraphExt
    {
        public static bool TryGetIncomingNode(this IScadGraph self,  ScadNode node, int port, out ScadNode result)
        {
            foreach (var connection in self.GetAllConnections().Where(connection => connection.To == node && connection.ToPort == port))
            {
                result = connection.From;
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryGetOutgoingNode(this IScadGraph self, ScadNode node, int port, out ScadNode result)
        {
            foreach (var connection in self.GetAllConnections().Where(connection => connection.From == node && connection.FromPort == port))
            {
                result = connection.To;
                return true;
            }

            result = default;
            return false;
        }
    }
}