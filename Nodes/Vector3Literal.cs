namespace OpenScadGraphEditor.Nodes
{
    public class Vector3Literal : IScadLiteral
    {
        public double X { get;  set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public string LiteralValue => $"[{X}, {Y}, {Z}]";

        public string SerializedValue
        {
            get => $"{X}|{Y}|{Z}";
            set
            {
                var parts = value.Split('|');
                X = double.Parse(parts[0]);
                Y = double.Parse(parts[1]);
                Z = double.Parse(parts[2]);;
            }
        }
        
        

    }
}