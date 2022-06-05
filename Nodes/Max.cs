using System.Linq;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Maximum node.
    /// </summary>
    [UsedImplicitly]
    public class Max : ScadNode, IAmAnExpression, IHaveVariableInputSize
    {
        public override string NodeTitle => "Max";
        public override string NodeDescription => "Returns the maximum of the input values.";

        public int CurrentInputSize { get; private set; } = 1;

        public int InputPortOffset => 0;
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add input value";
        public string RemoveRefactoringTitle => "Remove input value";
        public bool OutputPortsMatchVariableInputs => false;

        public Max()
        {
            RebuildPorts();
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();

            OutputPorts
                .Clear();

            OutputPorts
                .Any();

            for (var i = 0; i < CurrentInputSize; i++)
            {
                if (CurrentInputSize == 1)
                {
                    InputPorts.Any($"Input {i + 1}");
                }
                else
                {
                    InputPorts.Number($"Input {i + 1}");
                }
            }
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                if (CurrentInputSize == 1)
                {
                    return "A vector of numbers or a single number.";
                }
                return "A number";
            }

            if (portId.IsOutput)
            {
                return  "The maximum of the input values.";
            }

            return "";
        }
        public void AddVariableInputPort()
        {
            CurrentInputSize += 1;
            RebuildPorts();
            // since we have no literals here, we can skip re-building port literals
        }

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
            var parameters = CurrentInputSize.Range()
                .Select(it => RenderInput(context, it).OrUndef())
                .JoinToString(", ");

            return $"max({parameters})";
        }
    }
}