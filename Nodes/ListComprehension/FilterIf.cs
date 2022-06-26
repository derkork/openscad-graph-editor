using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes.ConstructVector;

namespace OpenScadGraphEditor.Nodes.ListComprehension
{
    public class FilterIf : ScadNode, IAmAListComprehensionExpression
    {
        public override string NodeTitle => "Filter If";

        public override string NodeDescription =>
            "When the evaluation of the condition returns true, the expression in the true 'port' is added to the result list else the expression in the 'false' port. Can only be used in list comprehension expressions.";


        public FilterIf()
        {
            InputPorts
                .Boolean("Condition")
                .Any("True")
                .Any("False");

            OutputPorts
                .Any();
        }
        
        static FilterIf()
        {
            // the output of "FilterIf" can only be connected to another list comprehension or a vector construction
            ConnectionRules.AddConnectRule(
                it => it.From is Each && !(it.To is IAmAListComprehensionExpression || it.To is IAmAVectorConstruction),
                ConnectionRules.OperationRuleDecision.Veto
            );
        }

        
        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "Condition";
                case 1 when portId.IsInput:
                    return "Expression to add when condition is true";
                case 2 when portId.IsInput:
                    return "Expression to add when condition is false";
                case 0 when portId.IsOutput:
                    return "Result";
                default:
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex != 0)
            {
                return "";
            }
            
            var condition = RenderInput(context, 0);
            var ifExpression = RenderInput(context, 1);
            var elseExpression = RenderInput(context, 2);

            if (ifExpression.Empty() && elseExpression.Empty())
            {
                return "undef"; // no expression given so this is just undefined
            }

            if (ifExpression.Empty())
            {
                // we only got an else branch, so reverse the condition.
                return $"(if (!{condition}) {elseExpression})";
            }

            if (elseExpression.Empty())
            {
                // we only got an if branch
                return $"(if ({condition}) {ifExpression})";
            }
            
            // we got both
            return $"(if ({condition}) {ifExpression} else {elseExpression})";
        }
    }
}