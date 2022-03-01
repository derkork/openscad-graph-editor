using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class DivideOperator : BinaryNumericOperator
    {
        public override string NodeTitle => "/";
        public override string NodeDescription => "Divides the given inputs.";
        protected override string OperatorSign => "/";
    }
}