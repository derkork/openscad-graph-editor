using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    [UsedImplicitly]
    public class ConstructVectorNumber : ConstructVector
    {
        public override string NodeQuickLookup => "VCN";

        public ConstructVectorNumber() : base(PortType.Number)
        {
        }

        public override string NodeTitle => "Construct Vector (Number)";
        public override string NodeDescription => "Constructs a vector of numbers.";
    }
}