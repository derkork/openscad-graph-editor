using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ExponentiationOperator : BinaryOperator
    {
        public override string NodeTitle => "^";
        public override string NodeDescription => "Exponentiation of the given inputs.";
        protected override string OperatorSign => "^";

        public ExponentiationOperator()
        {
            InputPorts
                .Number()
                .Number();

            OutputPorts
                .Number();
        }
    }
}