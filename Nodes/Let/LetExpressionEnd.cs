using System.Text;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Let
{
    [UsedImplicitly]
    public class LetExpressionEnd : ScadNode, IAmAnExpression, IAmBoundToOtherNode
    {
        public override string NodeTitle => "End Let Expression";
        public override string NodeDescription  => "Collects the value of the let expression and returns it.";

        public string OtherNodeId { get; set; }

        public LetExpressionEnd()
        {
            InputPorts
                .Any();

            OutputPorts
                .Any();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "The expression of the let.";
            }

            if (portId.IsOutput)
            {
                return "The result of the let.";
            }
            
            return "";
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("letExpressionStartId", OtherNodeId);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            OtherNodeId = node.GetDataString("letExpressionStartId");
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex != 0)
            {
                return "";
            }

            var startNode = (LetExpressionStart) context.ById(OtherNodeId);
            
            var builder = new StringBuilder("let(");
            for (var i = 0; i < startNode.VariableCount; i++)
            {
                var variableName = startNode.Render(context, i);
                var expression = startNode.RenderInput(context,  i).OrUndef();
                builder.Append(variableName)
                    .Append(" = ")
                    .Append(expression);
                if (i + 1 < startNode.VariableCount)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(")");

            var resultExpression = RenderInput(context, 0);

            return $"{builder} {resultExpression}";
        }
    }
}