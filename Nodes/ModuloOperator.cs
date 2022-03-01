using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ModuloOperator : BinaryNumericOperator
    {
        public override string NodeTitle => "%";
        public override string NodeDescription => "Calculates the modulus the given inputs.";
        protected override string OperatorSign => "%";
    }
}