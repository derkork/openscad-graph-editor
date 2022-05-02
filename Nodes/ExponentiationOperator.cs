using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ExponentiationOperator : BinaryOperator
    {
        public override string NodeTitle => "^";
        public override string NodeDescription => "Exponentiation of the given inputs (first to the power of second).";
        protected override string OperatorSign => "^";

        public override Texture NodeBackground => Resources.ExpIcon;

        public ExponentiationOperator()
        {
            InputPorts
                .Number()
                .Number();

            OutputPorts
                .Number(allowLiteral: false);
        }
    }
}