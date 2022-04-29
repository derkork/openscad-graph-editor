namespace OpenScadGraphEditor.Nodes
{
    public class Vector2Literal : LiteralBase
    {
        public double X { get;  set; }
        public double Y { get; set; }

        public override string RenderedValue => $"[{X}, {Y}]";

        public override string SerializedValue
        {
            get => $"{X}|{Y}";
            set
            {
                var parts = value.Split('|');
                X = double.Parse(parts[0]);
                Y = double.Parse(parts[1]);
            }
        }
    }
}