using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class Vector3Literal : LiteralBase
    {
        public double X { get;  set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3Literal(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public override string RenderedValue => $"[{X.SafeToString()}, {Y.SafeToString()}, {Z.SafeToString()}]";

        public override string SerializedValue
        {
            get => $"{X.SafeToString()}|{Y.SafeToString()}|{Z.SafeToString()}";
            set
            {
                var parts = value.Split('|');
                X = parts[0].SafeParse();
                Y = parts[1].SafeParse();
                Z = parts[2].SafeParse();
            }
        }

        public override LiteralType LiteralType => LiteralType.Vector3;
    }
}