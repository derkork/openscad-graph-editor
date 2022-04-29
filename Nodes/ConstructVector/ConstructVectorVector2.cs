using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    [UsedImplicitly]
    public class ConstructVectorVector2 : ConstructVector
    {
        public ConstructVectorVector2() : base(PortType.Vector2)
        {
        }

        public override string NodeTitle => "Construct Vector (Vector2)";
        public override string NodeDescription => "Constructs a vector of Vector2s.";
    }
}