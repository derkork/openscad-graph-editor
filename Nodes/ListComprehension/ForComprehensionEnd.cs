using System.Text;
using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes.ConstructVector;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ListComprehension
{
    /// <summary>
    /// End part of the for-comprehension.
    /// </summary>
    [UsedImplicitly]
    public class ForComprehensionEnd : ScadNode, IAmBoundToOtherNode, IAmAListComprehensionExpression, ICannotBeCreated
    {
        public override string NodeTitle => "End For Comprehension";

        public override string NodeDescription =>
            "Collects the results of the comprehension and returns them as a list.";

        public string OtherNodeId { get; set; }

        public ForComprehensionEnd()
        {
            InputPorts
                .Any();
            OutputPorts
                .Array();
        }

        static ForComprehensionEnd()
        {
            // the output of "for comprehension end" can only be connected to another list comprehension or a vector construction
            ConnectionRules.AddConnectRule(
                it => it.From is ForComprehensionEnd && !(it.To is IAmAListComprehensionExpression || it.To is IAmAVectorConstruction),
                ConnectionRules.OperationRuleDecision.Veto
            );
        }

        
        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "An expression that makes up a list element.";
            }

            if (portId.IsOutput)
            {
                return "The generated list.";
            }

            return "";
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("comprehensionStartId", OtherNodeId);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            OtherNodeId = node.GetDataString("comprehensionStartId", "");
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex != 0)
            {
                return "";
            }

            var innerExpression = RenderInput(context, 0);
            if (innerExpression.Empty())
            {
                return "";
            }


            var startNode = (ForComprehensionStart) context.ById(OtherNodeId);

            var builder = new StringBuilder("for(");
            for (var i = 0; i < startNode.CurrentInputSize; i++)
            {
                var loopVarName = startNode.Render(context, i);
                var array = startNode.RenderInput(context, i).OrDefault("[]");
                builder.Append(loopVarName)
                    .Append(" = ")
                    .Append(array);
                if (i + 1 < startNode.CurrentInputSize)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(") ")
                .Append(innerExpression);
            return builder.ToString();
        }
    }
}