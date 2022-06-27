using System.Text;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Let
{
    [UsedImplicitly]
    public class LetBlockEnd : ScadNode, IAmBoundToOtherNode
    {
        public override string NodeTitle => "End Let";
        public override string NodeQuickLookup => "";
        public override string NodeDescription  => "Collects the geometry from the let block and returns it";

        public string OtherNodeId { get; set; }

        public LetBlockEnd()
        {
            InputPorts
                .Geometry();

            OutputPorts
                .Geometry();
        }


        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "Geometry from the let block";
            }

            if (portId.IsOutput)
            {
                return "The collected geometry";
            }
            
            return "";
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("let_block_start_id", OtherNodeId);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            OtherNodeId = node.GetDataString("let_block_start_id");
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex != 0)
            {
                return "";
            }

            var startNode = (LetBlockStart) context.ById(OtherNodeId);
            
            var builder = new StringBuilder("let(");
            for (var i = 0; i < startNode.CurrentInputSize; i++)
            {
                var variableName = startNode.Render(context, i);
                var expression = startNode.RenderInput(context,  i).OrUndef();
                builder.Append(variableName)
                    .Append(" = ")
                    .Append(expression);
                if (i + 1 < startNode.CurrentInputSize)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(")");

            var children = RenderInput(context, 0);

            return $"{builder}{children.AsBlock()}";
        }
    }
}