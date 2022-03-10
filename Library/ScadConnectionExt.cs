using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public static class ScadConnectionExt
    {
        public static bool InvolvesNode(this IScadConnection connection, ScadNode node)
        {
            return connection.From == node || connection.To == node;
        }

        public static bool IsFrom(this IScadConnection connection, ScadNode node, int port)
        {
            return connection.From == node && connection.FromPort == port;
        }

        public static bool IsTo(this IScadConnection connection, ScadNode node, int port)
        {
            return connection.To == node && connection.ToPort == port;
        }
    }
}