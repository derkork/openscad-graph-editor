using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ListComprehension
{
    /// <summary>
    /// For comprehension node for building lists from other lists.
    /// </summary>
    [UsedImplicitly]
    public class ForComprehensionStart : ScadNode, IAmBoundToOtherNode, IAmAListComprehensionExpression, IHaveVariableInputSize
    {
        public override string NodeTitle => "For Comprehension";
        public override string NodeQuickLookup => "Foco";
        public override string NodeDescription => "Maps a list or range into a new list. Also known as a 'for' list comprehension.";

        public int CurrentInputSize { get; private set; } = 1;
        public int InputPortOffset => 0;
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add loop nest level";
        public string RemoveRefactoringTitle => "Remove loop nest level";
        public bool OutputPortsMatchVariableInputs => true;

        public string OtherNodeId { get; set; }

        public ForComprehensionStart()
        {
            RebuildPorts();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                if (portId.Port < CurrentInputSize)
                {
                    return "A list or range to be mapped.";
                }
            }

            if (portId.IsOutput)
            {
                if (portId.Port < CurrentInputSize)
                {
                    return
                        "The current element of the input list. This can be used to create expressions that can be connected into the 'Result' input port.";
                }
            }

            return "";
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();

            OutputPorts
                .Clear();

            for (var i = 0; i < CurrentInputSize; i++)
            {
                InputPorts.Array();
                OutputPorts.PortType(PortType.Any, literalType:   LiteralType.Name);
            }
      
        }

        public void AddVariableInputPort()
        {
            CurrentInputSize += 1;
            RebuildPorts();
            BuildPortLiteral(PortId.Output(CurrentInputSize-1));
        }

        public void RemoveVariableInputPort()
        {
            GdAssert.That(CurrentInputSize > 1, "Cannot decrease nest level any further.");
            DropPortLiteral(PortId.Output(CurrentInputSize-1));
            CurrentInputSize -= 1;
            RebuildPorts();
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("nest_level", CurrentInputSize);
            node.SetData("comprehensionEndId", OtherNodeId);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            CurrentInputSize = node.GetDataInt("nest_level", 1);
            OtherNodeId = node.GetDataString("comprehensionEndId");
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