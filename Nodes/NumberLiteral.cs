using System.Globalization;

namespace OpenScadGraphEditor.Nodes
{
    public class NumberLiteral : LiteralBase
    {
        public NumberLiteral(double value)
        {
            Value = value;
        }

        public double Value { get; set; }

        public override string RenderedValue => Value.ToString(CultureInfo.InvariantCulture);

        public override string SerializedValue
        {
            get => Value.ToString(CultureInfo.InvariantCulture);
            set => Value = double.Parse(value);
        }
    }
}