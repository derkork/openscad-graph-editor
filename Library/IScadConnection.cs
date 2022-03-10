using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public interface IScadConnection
    {
        ScadNode From { get; }
        int FromPort { get; }
        ScadNode To { get; }
        int ToPort { get; }
    }
}