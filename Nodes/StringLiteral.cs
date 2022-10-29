namespace OpenScadGraphEditor.Nodes
{
    public class StringLiteral : LiteralBase
    {
        public StringLiteral(string value)
        {
            Value = value;
        }

        public string Value { get; set; }

        public override string RenderedValue => "\"" + Value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

        public override string SerializedValue
        {
            get => Value;
            set => Value = value;
        }

        public override LiteralType LiteralType => LiteralType.String;
    }
}