using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class DivideOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        public override string NodeTitle => "/";
        public override string NodeDescription => "Divides the given inputs.";
        protected override string OperatorSign => "/";
    }
}