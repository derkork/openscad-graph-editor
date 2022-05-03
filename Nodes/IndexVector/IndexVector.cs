using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Nodes.IndexVector
{
    /// <summary>
    /// Node which allows to access vector indices.
    /// </summary>
    [UsedImplicitly]
    public class IndexVector : ScadNode, IAmAnExpression, IHaveMultipleExpressionOutputs
    {
        public override string NodeTitle => "Index Vector";
        public override string NodeDescription => "Returns the value of the vector at the given index";

        public int IndexPortCount { get; private set; } = 1;

        public override string Render(IScadGraph context)
        {
            GdAssert.That(false, "Cannot render this node.");
            return "";
        }

        public IndexVector()
        {
            RebuildPorts();
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("ports", IndexPortCount);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            IndexPortCount = node.GetDataInt("ports", 1);
            RebuildPorts();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void IncreasePorts()
        {
            IndexPortCount++;
            RebuildPorts();
            
            // build an input port literal
            BuildPortLiteral(PortId.Input(IndexPortCount));
            // build an output port literal
            BuildPortLiteral(PortId.Output(IndexPortCount-1));
        }

        public void DecreasePorts()
        {
            GdAssert.That(IndexPortCount > 1, "Cannot decrease ports below 1.");
            DropPortLiteral(PortId.Input(IndexPortCount));
            var idx = IndexPortCount - 1;
            DropPortLiteral(PortId.Output(idx));

            IndexPortCount--;
            RebuildPorts();
        }

        private void RebuildPorts()
        {
            InputPorts.Clear();
            OutputPorts.Clear();

            InputPorts
                .Array("Vector");
            
            for (var i = 0; i < IndexPortCount; i++)
            {
                InputPorts
                    .Number($"Index {i + 1}");
                OutputPorts
                    .Any($"Value {i + 1}");
            }
        }

        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            GdAssert.That(port >= 0 && port < IndexPortCount, "Port index out of range.");
            var vector = RenderInput(context, 0);
            var index = RenderInput(context, port+1);
            return $"({vector})[{index}]";
        }
    }
}