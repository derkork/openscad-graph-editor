using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    [UsedImplicitly]
    public class ConstructVectorVector3 : ConstructVector
    {
        public override string NodeQuickLookup => "VCV3";

        public ConstructVectorVector3() : base(PortType.Vector3)
        {
        }

        public override string NodeTitle => "Construct Vector (Vector3)";
        public override string NodeDescription => "Constructs a vector of Vector3s.";
    }
}