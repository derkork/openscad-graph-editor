using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Operator for negating a value.
    /// </summary>
    [UsedImplicitly]
    public class NegateOperator : ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Negate";
        public override string NodeDescription => "Returns the negative of the input.";


        public NegateOperator()
        {
            InputPorts
                .Number(allowLiteral:false);

            OutputPorts
                .Number(allowLiteral:false);
        }

        public override string Render(IScadGraph context)
        {
            var value = RenderInput(context, 0);
            return value.Empty() ? "" : $"-{value}";
        }
    }
}