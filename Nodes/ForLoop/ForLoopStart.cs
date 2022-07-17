using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ForLoop
{
    [UsedImplicitly]
    public class ForLoopStart : ScadNode, IAmBoundToOtherNode, IHaveVariableInputSize
    {
        public override string NodeTitle => "Start Loop" + (IntersectMode ? " (Intersect)" : "");
        public override string NodeQuickLookup => "Frlp";
        public override string NodeDescription => "Begins a loop. The loop will be executed for each element in the given vector.";
        
        public int CurrentInputSize { get; private set; } = 1;
        public int InputPortOffset => 0;
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add loop nest level";
        public string RemoveRefactoringTitle => "Remove loop nest level";
        public bool OutputPortsMatchVariableInputs => true;

        
        public bool IntersectMode { get; set; }

        public string OtherNodeId { get; set; }

        public ForLoopStart()
        {
            RebuildPorts();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "Vector to iterate over";
            }

            if (portId.IsOutput)
            {
                return "The current element of the vector from the left. You may give it a custom name. If you don't give it a name, one will be generated.";
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
                OutputPorts.OfType(PortType.Any, literalType: LiteralType.Name);
            }
        }

        public void AddVariableInputPort()
        {
            CurrentInputSize += 1;
            RebuildPorts();
            // add a port literal for the new variable
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
            node.SetData("intersect_mode", IntersectMode);
            node.SetData("loop_end_id", OtherNodeId);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            CurrentInputSize = node.GetDataInt("nest_level", 1);
            IntersectMode = node.GetDataBool("intersect_mode");
            OtherNodeId = node.GetDataString("loop_end_id");
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