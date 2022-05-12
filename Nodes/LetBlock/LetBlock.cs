using System.Text;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.LetBlock
{
    [UsedImplicitly]
    public class LetBlock : ScadNode, IHaveMultipleExpressionOutputs
    {
        public override string NodeTitle => "Let (Block)";
        public override string NodeDescription  => "Allows to define temporary variables";
         public int VariableCount { get; private set; } = 1;

        public LetBlock()
        {
            RebuildPorts();
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();
            InputPorts
                .Flow();

            OutputPorts
                .Clear();
            OutputPorts
                .Flow("Children");

            for (var i = 0; i < VariableCount; i++)
            {
                InputPorts.Any();
                OutputPorts.OfType(PortType.Any, literalType: LiteralType.Name, autoSetLiteralWhenPortIsDisconnected: true);
            }

            OutputPorts
                .Flow("After");
        }

        /// <summary>
        /// Adds a slot for a variable.
        /// </summary>
        public void IncreaseVariableCount()
        {
            VariableCount += 1;
            RebuildPorts();
            
            // add a port literal for the new variable
            BuildPortLiteral(PortId.Output(VariableCount));
        }

        /// <summary>
        /// Removes a slot for a variable.
        /// </summary>
        public void DecreaseVariableCount()
        {
            GdAssert.That(VariableCount > 1, "Cannot decrease nest level any further.");
            // remove the port literal for the removed variable
            DropPortLiteral(PortId.Output(VariableCount));
            VariableCount -= 1;
            RebuildPorts();
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("variable_count", VariableCount);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            VariableCount = node.GetDataInt("variable_count", 1);
            RebuildPorts();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(IScadGraph context)
        {
            var builder = new StringBuilder("let(");
            for (var i = 0; i < VariableCount; i++)
            {
                var variableName = RenderExpressionOutput(context, i + 1);
                var expression = RenderInput(context,  i + 1).OrUndef();
                builder.Append(variableName)
                    .Append(" = ")
                    .Append(expression);
                if (i + 1 < VariableCount)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(")");

            var children = RenderOutput(context, 0);
            var next = RenderOutput(context, 1+VariableCount);

            return $"{builder}{children.AsBlock()}\n{next}";
        }

        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            GdAssert.That(port > 0 && port <= VariableCount, "port out of range");
            return RenderOutput(context, port).OrDefault(Id.UniqueStableVariableName(port - 1));
        }
    }
}