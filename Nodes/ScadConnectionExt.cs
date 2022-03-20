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
    }
}