using System.Text;
using GodotExt;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    public abstract class ConstructVector : ScadExpressionNode
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


        public override void SaveInto(SavedNode node)
        {
            node.SetData("vector_size", VectorSize);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            VectorSize = node.GetDataInt("vector_size", 1);
            RebuildInputs();
        }

        public void IncreaseVectorSize()
        {
            VectorSize++;
            RebuildInputs();
            BuildPortLiteral(PortId.Input(VectorSize-1));
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
            // add an Any port for each vector component
            // since "Any" ports don't have literals, we don't need to rebuild literal values.
            for (var i = 0; i < VectorSize; i++)
            {
                InputPorts
                    .OfType(_portType, $"Component {i + 1}");
            }
        }


        public override string Render(IScadGraph context)
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