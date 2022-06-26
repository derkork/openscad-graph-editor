using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class ScadConnection
    {
        public ScadGraph Owner { get; }
        
        public ScadNode From { get; }
        public int FromPort { get; }
        public ScadNode To { get; }
        public int ToPort { get; }

        public ScadConnection(ScadGraph owner, ScadNode from, int fromPort, ScadNode to, int toPort)
        {
            Owner = owner;
            From = from;
            FromPort = fromPort;
            To = to;
            ToPort = toPort;
        }
    }
}