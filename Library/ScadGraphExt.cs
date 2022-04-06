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

        /// <summary>
        /// Returns true, if the graph contains references to the given invokable.
        /// </summary>
        public static bool ContainsReferencesTo(this IScadGraph self, InvokableDescription description)
        {
            return self.GetAllNodes().Any(it =>
                it is IReferToAnInvokable referToAnInvokable && referToAnInvokable.InvokableDescription == description);
        }
        
        /// <summary>
        /// Returns true, if the graph contains references to the given variable.
        /// </summary>
        public static bool ContainsReferencesTo(this IScadGraph self, VariableDescription description)
        {
            return self.GetAllNodes().Any(it =>
                it is IReferToAVariable referToAVariable && referToAVariable.VariableDescription == description);
        }
    }
}