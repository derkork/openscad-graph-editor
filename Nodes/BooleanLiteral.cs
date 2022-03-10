namespace OpenScadGraphEditor.Nodes
{
    public class BooleanLiteral : IScadLiteral
    {
        public bool Value { get; set; }

        public string LiteralValue => Value ? "true" : "false";

        public string SerializedValue
        {
            get => Value ? "true" : "false";
            set => Value = value == "true";
        }
    }
}