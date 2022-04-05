using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class TernaryOperator : ScadExpressionNode
    {
        public override string NodeTitle => "Conditional if/else (?:)";
        public override string NodeDescription => "A function that uses a test to determine which of 2 values to return. Also known as the ternary operator.";


        public TernaryOperator()
        {
            InputPorts
                .Boolean("Condition")
                .Any("True Value")
                .Any("False Value");

            OutputPorts
                .Any();

        }
        
        public override string Render(IScadGraph context)
        {
            var condition = RenderInput(context, 0);
            var trueValue = RenderInput(context, 1);
            var falseValue = RenderInput(context, 2);
            
            return $"({condition.OrUndef()}) ? ({trueValue.OrUndef()}) : ({falseValue.OrUndef()})";
        }
    }
}