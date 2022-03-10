namespace OpenScadGraphEditor.Nodes
{
    public interface IScadLiteral
    {
        string LiteralValue { get; }
        string SerializedValue { get; set; }
    }
}