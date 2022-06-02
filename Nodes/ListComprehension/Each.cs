using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes.ConstructVector;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ListComprehension
{
    public class Each : ScadNode, IAmAListComprehensionExpression

    {
        public override string NodeTitle => "Each";
        public override string NodeDescription => "Unwraps a list and inlines its elements into the current scope. Can only be used in a for comprehension or a vector construction.";

        public Each()
        {
            InputPorts
                .Array();
            OutputPorts
                .Any();
        }

        static Each()
        {
            // the output of "each" can only be connected to another list comprehension or a vector construction
            ConnectionRules.AddConnectRule(
                it => it.From is Each && !(it.To is IAmAListComprehensionExpression || it.To is IAmAVectorConstruction),
                ConnectionRules.OperationRuleDecision.Veto
            );
        }
        
        
        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "The list to unwrap.";
            }

            if (portId.IsOutput)
            {
                return "The unwrapped elements.";
            }

            return "";
        }


        public override string Render(ScadGraph context, int portIndex)
        {
            var listExpression = RenderInput(context, 0).OrDefault("[]");
            return $"each {listExpression}";
        }
    }
}