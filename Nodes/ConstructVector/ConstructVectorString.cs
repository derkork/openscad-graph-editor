using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    [UsedImplicitly]
    public class ConstructVectorString : ConstructVector
    {
        public override string NodeQuickLookup => "VCS";

        public ConstructVectorString() : base(PortType.String)
        {
        }

        public override string NodeTitle => "Construct Vector (String)";
        public override string NodeDescription => "Constructs a vector of strings.";
    }
}