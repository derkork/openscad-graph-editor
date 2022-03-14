using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    internal class ScadConnection : IScadConnection
    {
        public ScadNode From { get; }
        public int FromPort { get; }
        public ScadNode To { get; }
        public int ToPort { get; }

        public ScadConnection(ScadNode from, int fromPort, ScadNode to, int toPort)
        {
            From = from;
            FromPort = fromPort;
            To = to;
            ToPort = toPort;
        }
    }
}