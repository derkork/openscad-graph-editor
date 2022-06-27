using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Let
{
    [UsedImplicitly]
    public class LetBlockStart : ScadNode, IAmBoundToOtherNode, IHaveVariableInputSize
    {
        public override string NodeTitle => "Let";
        public override string NodeQuickLookup => "Ltbl";
        public override string NodeDescription  => "Allows to define temporary variables";

        public string OtherNodeId { get; set; }
        public int CurrentInputSize { get; private set; } = 1;
         public int InputPortOffset => 0;
         public int OutputPortOffset => 0;
         public string AddRefactoringTitle => "Add variable";
         public string RemoveRefactoringTitle => "Remove variable";
         public bool OutputPortsMatchVariableInputs => true;

         public LetBlockStart()
        {
            RebuildPorts();
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();

            OutputPorts
                .Clear();

            for (var i = 0; i < CurrentInputSize; i++)
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

        public void AddVariableInputPort()
        {
            CurrentInputSize += 1;
            RebuildPorts();
            
            // add a port literal for the new variable
            BuildPortLiteral(PortId.Output(CurrentInputSize));
        }

        public void RemoveVariableInputPort()
        {
            GdAssert.That(CurrentInputSize > 1, "Cannot decrease nest level any further.");
            // remove the port literal for the removed variable
            DropPortLiteral(PortId.Output(CurrentInputSize));
            CurrentInputSize -= 1;
            RebuildPorts();
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("variable_count", CurrentInputSize);
            node.SetData("let_block_end_id", OtherNodeId);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            CurrentInputSize = node.GetDataInt("variable_count", 1);
            OtherNodeId = node.GetDataString("let_block_end_id");
            RebuildPorts();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex < 0 || portIndex >= CurrentInputSize)
            {
                return "";
            }
            return RenderOutput(context, portIndex).OrDefault(Id.UniqueStableVariableName(portIndex));

        }

    }
}