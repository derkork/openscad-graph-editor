using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class AddOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        public override string NodeTitle => "+";
        public override string NodeDescription => "Adds the given inputs.";
        protected override string OperatorSign => "+";
    }
}