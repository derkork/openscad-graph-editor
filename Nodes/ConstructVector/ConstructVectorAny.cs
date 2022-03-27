using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    [UsedImplicitly]
    public class ConstructVectorAny : ConstructVector
    {
        public ConstructVectorAny() : base(PortType.Any)
        {
        }

        public override string NodeTitle => "Construct Vector (Any)";
        public override string NodeDescription => "Constructs a vector of arbitrary elements.";
    }
}