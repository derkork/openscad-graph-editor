using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class MultiplyOperator : BinaryNumericOperator
    {
        public override string NodeTitle => "*";
        public override string NodeDescription => "Multiplies the given inputs.";
        protected override string OperatorSign => "+";
    }
}