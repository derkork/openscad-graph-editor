using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Operator for negating a value.
    /// </summary>
    [UsedImplicitly]
    public class NegateOperator : ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Negate";
        public override string NodeQuickLookup => "Negg";
        public override string NodeDescription => "Returns the negative of the input.";


        public NegateOperator()
        {
            InputPorts
                .Any();

            OutputPorts
                .Any();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The value to negate.";
                case 0 when portId.IsOutput:
                    return "The negative of the input.";
                default: 
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var value = RenderInput(context, 0);
            return value.Empty() ? "" : $"(-{value})";
        }
    }
}