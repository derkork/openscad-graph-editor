using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Ternary operator node.
    /// </summary>
    [UsedImplicitly]
    public class TernaryOperator : ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Conditional if/else (?:)";
        public override string NodeDescription => "Checks a condition to determine which of 2 values to return. Also known as the ternary operator.";

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                switch (portId.Port)
                {
                    case 0:
                        return "The condition that should be checked.";
                    case 1:
                        return "The value to return if the condition is true.";
                    case 2:
                        return "The value to return if the condition is false.";
                }
            }

            if (portId.IsOutput)
            {
                if (portId.Port == 0)
                {
                    return "The result of the conditional operator.";
                }
            }

            return "";
        }


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
            
            return $"({condition.OrUndef()} ? {trueValue.OrUndef()} : {falseValue.OrUndef()})";
        }
    }
}