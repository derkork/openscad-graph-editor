using System.Text;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ForLoop
{
    [UsedImplicitly]
    public class ForLoop : ScadNode, IHaveMultipleExpressionOutputs
    {
        public override string NodeTitle => "For Each";
        public override string NodeDescription => "Executes its children for each entry in the given vector.";

        public int NestLevel { get; private set; } = 1;

        public ForLoop()
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
                .Flow("Children");

            for (var i = 0; i < NestLevel; i++)
            {
                InputPorts.Array($"Vector {i + 1}");
                OutputPorts.Any($"Vector Element {i + 1}");
            }

            OutputPorts
                .Flow("After");
        }

        /// <summary>
        /// Adds a nested for loop level. The caller is responsible for fixing up port connections.
        /// </summary>
        public void IncreaseNestLevel()
        {
            NestLevel += 1;
            RebuildPorts();
            // since we have no literals here, we can skip re-building port literals
        }

        /// <summary>
        /// Removes a nested for loop level. The caller is responsible for fixing up port connections.
        /// </summary>
        public void DecreaseNestLevel()
        {
            GdAssert.That(NestLevel > 1, "Cannot decrease nest level any further.");
            NestLevel -= 1;
            RebuildPorts();
            // since we have no literals here, we can skip re-building port literals
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("nest_level", NestLevel);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            NestLevel = node.GetDataInt("nest_level", 1);
            RebuildPorts();
        }

        public override string Render(IScadGraph context)
        {
            var builder = new StringBuilder("for(");
            for (var i = 0; i < NestLevel; i++)
            {
                var loopVarName = Id.UniqueStableVariableName(i);
                var array = RenderInput(context, 1 + i);
                builder.Append(loopVarName)
                    .Append(" = ")
                    .Append(array);
                if (i + 1 < NestLevel)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(")");

            var children = RenderOutput(context, 0);
            var next = RenderOutput(context, 1+NestLevel);

            return $"{builder}{children.AsBlock()}\n{next}";
        }

        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            GdAssert.That(port > 0 && port <= NestLevel, "port out of range");
            return Id.UniqueStableVariableName(port - 1);
        }
    }
}