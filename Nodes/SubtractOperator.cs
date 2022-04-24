using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class SubtractOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        public override string NodeTitle => "-";
        public override string NodeDescription => "Subtracts the given inputs.";
        protected override string OperatorSign => "-";
    }
}