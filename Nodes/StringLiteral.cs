namespace OpenScadGraphEditor.Nodes
{
    public class StringLiteral : IScadLiteral
    {
        public string Value { get; set; }

        public string LiteralValue => "\"" + Value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        public string SerializedValue
        {
            get => Value;
            set => Value = value;
        }
    }
}