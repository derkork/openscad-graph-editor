using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class Vector2Literal : LiteralBase
    {
        public double X { get;  set; }
        public double Y { get; set; }
        
        public Vector2Literal(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string RenderedValue => $"[{X.SafeToString()}, {Y.SafeToString()}]";

        public override string SerializedValue
        {
            get => $"{X.SafeToString()}|{Y.SafeToString()}";
            set
            {
                var parts = value.Split('|');
                X = parts[0].SafeParse();
                Y = parts[1].SafeParse();
            }
        }

        public override LiteralType LiteralType => LiteralType.Vector2;
    }
}