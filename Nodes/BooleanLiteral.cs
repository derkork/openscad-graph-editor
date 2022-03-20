namespace OpenScadGraphEditor.Nodes
{
    public class BooleanLiteral : IScadLiteral
    {
        public bool Value { get; set; }

        public BooleanLiteral(bool value)
        {
            Value = value;
        }

        public string LiteralValue => Value ? "true" : "false";

        public string SerializedValue
        {
            get => Value ? "true" : "false";
            set => Value = value == "true";
        }
    }
}