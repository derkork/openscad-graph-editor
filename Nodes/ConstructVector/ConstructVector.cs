using System.Text;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    public abstract class ConstructVector : ScadNode, IAmAnExpression, IAmAVectorConstruction, IHaveVariableInputSize
    {
        private readonly PortType _portType;

        public int CurrentInputSize { get; private set; } = 1;
        public int InputPortOffset => 0;
        public int OutputPortOffset => 0;
        public string AddRefactoringTitle => "Add item";
        public string RemoveRefactoringTitle => "Remove item";
        public bool OutputPortsMatchVariableInputs => false;

        public ConstructVector(PortType portType)
        {
            _portType = portType;
            RebuildInputs();
            OutputPorts
                .Array();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return "An element to be added to the vector";
            }

            if (portId.IsOutput)
            {
                return "The constructed vector";
            }

            return "";
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("vector_size", CurrentInputSize);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            CurrentInputSize = node.GetDataInt("vector_size", 1);
            RebuildInputs();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void AddVariableInputPort()
        {
            CurrentInputSize++;
            RebuildInputs();
            BuildPortLiteral(PortId.Input(CurrentInputSize-1));
            
            // as a convenience copy the literal set style of the existing one, this makes it easier for the user.
            if (TryGetLiteral(PortId.Input(CurrentInputSize - 2), out var previousLiteral) && TryGetLiteral(PortId.Input(CurrentInputSize-1), out var newLiteral))
            {
                newLiteral.IsSet = previousLiteral.IsSet;
            }
            
        }

        public void RemoveVariableInputPort()
        {
            GdAssert.That(CurrentInputSize > 1, "Cannot decrease vector size below 1.");
            var idx = CurrentInputSize-1;

            DropPortLiteral(PortId.Input(idx));

            CurrentInputSize--;
            RebuildInputs();
        }

        private void RebuildInputs()
        {
            InputPorts.Clear();
            for (var i = 0; i < CurrentInputSize; i++)
            {
                InputPorts
                    .PortType(_portType, $"Component {i + 1}", _portType.GetMatchingLiteralType());
            }
        }


        public override string Render(ScadGraph context, int portIndex)
        {
            var builder = new StringBuilder();

            // render all input ports and combine their results into a vector
            for (var i = 0; i < CurrentInputSize; i++)
            {
                var part = RenderInput(context, i);
                builder.Append(part);
                if (i + 1 < CurrentInputSize)
                {
                    builder.Append(", ");
                }
            }

            return $"[{builder}]";
        }
    }
}