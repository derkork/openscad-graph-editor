using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class SubtractOperator : BinaryAutoCoercingNumberOperator
    {
        public override string NodeTitle => "-";
        public override string NodeDescription => "Subtracts the given inputs.";
        protected override string OperatorSign => "-";
    }
}