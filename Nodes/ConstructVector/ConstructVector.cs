using System.Text;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    public abstract class ConstructVector : ScadNode, IAmAnExpression, IAmAVectorConstruction
    {
        private readonly PortType _portType;

        public int VectorSize { get; private set; } = 1;


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
            node.SetData("vector_size", VectorSize);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            VectorSize = node.GetDataInt("vector_size", 1);
            RebuildInputs();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void IncreaseVectorSize()
        {
            VectorSize++;
            RebuildInputs();
            BuildPortLiteral(PortId.Input(VectorSize-1));
            
            // as a convenience copy the literal set style of the existing one, this makes it easier for the user.
            if (TryGetLiteral(PortId.Input(VectorSize - 2), out var previousLiteral) && TryGetLiteral(PortId.Input(VectorSize-1), out var newLiteral))
            {
                newLiteral.IsSet = previousLiteral.IsSet;
            }
            
        }

        public void DecreaseVectorSize()
        {
            GdAssert.That(VectorSize > 1, "Cannot decrease vector size below 1.");
            var idx = VectorSize-1;

            DropPortLiteral(PortId.Input(idx));

            VectorSize--;
            RebuildInputs();
        }

        private void RebuildInputs()
        {
            InputPorts.Clear();
            for (var i = 0; i < VectorSize; i++)
            {
                InputPorts
                    .OfType(_portType, $"Component {i + 1}", _portType.GetMatchingLiteralType());
            }
        }


        public override string Render(ScadGraph context, int portIndex)
        {
            var builder = new StringBuilder();

            // render all input ports and combine their results into a vector
            for (var i = 0; i < VectorSize; i++)
            {
                var part = RenderInput(context, i);
                builder.Append(part);
                if (i + 1 < VectorSize)
                {
                    builder.Append(", ");
                }
            }

            return $"[{builder}]";
        }
    }
}