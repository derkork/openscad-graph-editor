using System.Linq;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Str : ScadNode, IAmAnExpression, IHaveVariableInputSize
    {
        public override string NodeTitle => "Str";
        public override string NodeDescription => "Convert all arguments to string and concatenate them.";
        public override string NodeQuickLookup => "ToSt";

        public int CurrentInputSize { get; private set; } = 1;
        public int InputPortOffset => 0;
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add input value";
        public string RemoveRefactoringTitle => "Remove input value";
        public bool OutputPortsMatchVariableInputs => false;

        public Str()
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
                .String(allowLiteral: false);

            for (var i = 0; i < CurrentInputSize; i++)
            {
                InputPorts.Any($"Input {i + 1}");
            }
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "An expression to add to the string.";
            }

            if (portId.IsOutput)
            {
                return "The concatenated string.";
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

            return $"str({parameters})";
        }
    }
}