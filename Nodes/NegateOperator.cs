using Godot;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public class NegateOperator : ScadExpressionNode
    {
        public override string NodeTitle => "Negate";
        public override string NodeDescription => "Returns the negative of the input";


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