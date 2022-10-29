namespace OpenScadGraphEditor.Nodes
{
    public class BooleanLiteral : LiteralBase
    {
        public bool Value { get; set; }

        public BooleanLiteral(bool value)
        {
            Value = value;
        }

        public override string RenderedValue => Value ? "true" : "false";

        public override string SerializedValue
        {
            get => Value ? "true" : "false";
            set => Value = value == "true";
        }

        public override LiteralType LiteralType => LiteralType.Boolean;
    }
}