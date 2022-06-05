using System.Linq;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Echo : ScadNode,  IHaveVariableInputSize
    {
        public override string NodeTitle => "Echo";
        public override string NodeDescription => "Writes one or more values to the console";

        public int CurrentInputSize { get; private set; } = 1;
        public int InputPortOffset => 0;
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add input port";
        public string RemoveRefactoringTitle => "Remove input port";
        public bool OutputPortsMatchVariableInputs => false;


        public Echo()
        {
            RebuildPorts();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "A value that will be written to the console";
            }

            if (portId.IsOutput)
            {
                return "Output flow";
            }

            return "";
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();

            OutputPorts
                .Clear();
            OutputPorts
                .Geometry();

            for (var i = 0; i < CurrentInputSize; i++)
            {
                InputPorts.Any($"Input {i + 1}");
            }

        }

        /// <summary>
        /// Adds a new input. The caller is responsible for fixing up port connections.
        /// </summary>
        public void AddVariableInputPort()
        {
            CurrentInputSize += 1;
            RebuildPorts();
            // since we have no literals here, we can skip re-building port literals
        }

        /// <summary>
        /// Removes an input. The caller is responsible for fixing up port connections.
        /// </summary>
        public void RemoveVariableInputPort()
        {
            GdAssert.That(CurrentInputSize > 1, "Cannot decrease nest inputs any further.");
            CurrentInputSize -= 1;
            RebuildPorts();
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("input_count", CurrentInputSize);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            CurrentInputSize = node.GetDataInt("input_count", 1);
            RebuildPorts();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex != 0)
            {
                return "";
            }
            
            var parameters = CurrentInputSize.Range()
                .Select(it => RenderInput(context, it ).OrUndef())
                .JoinToString(", ");

            return $"echo({parameters});";
        }
    }
}