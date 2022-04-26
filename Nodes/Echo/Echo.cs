using System.Text;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Echo
{
    [UsedImplicitly]
    public class Echo : ScadNode
    {
        public override string NodeTitle => "Echo";
        public override string NodeDescription => "Writes one or more values to the console";

        public int InputCount { get; private set; } = 1;

        public Echo()
        {
            RebuildPorts();
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();
            InputPorts
                .Flow();

            OutputPorts
                .Clear();
            OutputPorts
                .Flow();

            for (var i = 0; i < InputCount; i++)
            {
                InputPorts.Any($"Input {i + 1}");
            }

        }

        /// <summary>
        /// Adds a new input. The caller is responsible for fixing up port connections.
        /// </summary>
        public void AddInput()
        {
            InputCount += 1;
            RebuildPorts();
            // since we have no literals here, we can skip re-building port literals
        }

        /// <summary>
        /// Removes an input. The caller is responsible for fixing up port connections.
        /// </summary>
        public void RemoveInput()
        {
            GdAssert.That(InputCount > 1, "Cannot decrease nest inputs any further.");
            InputCount -= 1;
            RebuildPorts();
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("input_count", InputCount);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            InputCount = node.GetDataInt("input_count", 1);
            RebuildPorts();
        }

        public override string Render(IScadGraph context)
        {
            var builder = new StringBuilder("echo(");
            for (var i = 0; i < InputCount; i++)
            {
                var expression = RenderInput(context, 1 + i).OrUndef();
                builder.Append(expression);
                if (i + 1 < InputCount)
                {
                    builder.Append(", ");
                }
            }
            builder.Append(")");

            var next = RenderOutput(context, 0);

            return $"{builder};\n{next}";
        }
    }
}