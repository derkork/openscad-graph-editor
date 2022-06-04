using System.Text;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Let
{
    [UsedImplicitly]
    public class LetExpressionStart : ScadNode, IAmAnExpression, IAmBoundToOtherNode
    {
        public override string NodeTitle => "Let";
        public override string NodeDescription  => "Allows to define temporary variables inside of an expression";
        public int VariableCount { get; private set; } = 1;

        public string OtherNodeId { get; set; }

        public LetExpressionStart()
        {
            RebuildPorts();
        }

        
        
        private void RebuildPorts()
        {
            InputPorts
                .Clear();

            OutputPorts
                .Clear();

            for (var i = 0; i < VariableCount; i++)
            {
                InputPorts.Any();
                OutputPorts.OfType(PortType.Any, literalType: LiteralType.Name, autoSetLiteralWhenPortIsDisconnected: true);
            }
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "An expression which is assigned to the variable.";
            }

            if (portId.IsOutput)
            {
                return "The declared variable's value.";
            }
            
            return "";
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

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex < 0 || portIndex >= VariableCount)
            {
                return "";
            }
            
            return RenderOutput(context, portIndex).OrDefault(Id.UniqueStableVariableName(portIndex - 1));
        }
    }
}