using System.Linq;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Vector concatenation node.
    /// </summary>
    [UsedImplicitly]
    public class Concat : ScadNode, IAmAnExpression, IHaveVariableInputSize
    {
        public override string NodeTitle => "Concat";
        public override string NodeDescription => "Return a new vector that is the result of appending the supplied elements.";

        // variable size properties
        public int InputPortOffset => 0;
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add expression";
        public string RemoveRefactoringTitle => "Remove expression";
        public bool OutputPortsMatchVariableInputs => false;
        public int CurrentInputSize { get; private set; } = 1;


        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "An element to append to the vector.";
            }
            
            if (portId.IsOutput)
            {
                return "The resulting vector.";
            }

            return "";
        }

        public Concat()
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
                .Array();

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
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var parameters = CurrentInputSize.Range()
                .Select(it => RenderInput(context, it).OrUndef())
                .JoinToString(", ");

            return $"concat({parameters})";
        }
    }
}