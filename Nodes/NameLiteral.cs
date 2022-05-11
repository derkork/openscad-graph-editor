namespace OpenScadGraphEditor.Nodes
{
    public class NameLiteral : LiteralBase
    {
        public NameLiteral(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
        
        public override string RenderedValue =>  Value;

        public override string SerializedValue
        {
            get => Value;
            set => Value = value;
        }
    }
}