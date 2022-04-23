namespace OpenScadGraphEditor.Nodes
{
    public interface IScadLiteral
    {
        /// <summary>
        /// Indicator whether the user actually wants to set this literal.
        /// </summary>
        bool IsSet { get; set; }
        string RenderedValue { get; }
        string SerializedValue { get; set; }
    }
}