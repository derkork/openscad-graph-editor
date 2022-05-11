namespace OpenScadGraphEditor.Nodes
{
    public abstract class LiteralBase : IScadLiteral
    {
        public virtual bool IsSet { get; set; }
        public abstract string RenderedValue { get; }
        public abstract string SerializedValue { get; set; }
    }
}