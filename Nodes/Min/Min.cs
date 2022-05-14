using System.Linq;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Min
{
    /// <summary>
    /// Minimum node.
    /// </summary>
    [UsedImplicitly]
    public class Min : ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Min";
        public override string NodeDescription => "Returns the minimum of the input values.";

        public int InputCount { get; private set; } = 1;

        public Min()
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

            for (var i = 0; i < InputCount; i++)
            {
                if (InputCount == 1)
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
                if (InputCount == 1)
                {
                    return "A vector of numbers or a single number.";
                }
                return "A number";
            }

            if (portId.IsOutput)
            {
                return  "The minimum of the input values.";
            }

            return "";
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
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(IScadGraph context)
        {
            var parameters = InputCount.Range()
                .Select(it => RenderInput(context, it).OrUndef())
                .JoinToString(", ");

            return $"min({parameters})";
        }
    }
}