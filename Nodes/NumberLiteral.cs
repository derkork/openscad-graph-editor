using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class NumberLiteral : LiteralBase
    {
        public NumberLiteral(double value)
        {
            Value = value;
        }

        public double Value { get; set; }

        public override string RenderedValue => Value.SafeToString();

        public override string SerializedValue
        {
            get => Value.SafeToString();
            set => Value = value.SafeParse();
        }

        public override LiteralType LiteralType => LiteralType.Number;
    }
}