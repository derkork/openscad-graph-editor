using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Difference : ScadNode, ICanHaveModifier
    {
        public override string NodeTitle => "Difference";

        public override string NodeDescription =>
            "Subtracts the child nodes from the first one (logical and not).";

        public Difference()
        {
            InputPorts
                .Flow();

            OutputPorts
                .Flow("Input")
                .Flow("Subtract")
                .Flow("After");
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "Input flow";
                case 0 when portId.IsOutput:
                    return "The geometry from which the other geometry should be subtracted.";
                case 1 when portId.IsOutput:
                    return "The geometry that should be subtracted from the first geometry.";
                case 2 when portId.IsOutput:
                    return "Output flow";
                default:
                    return "";
            }
        }

        public override string Render(IScadGraph context)
        {
            var first = $"union(){RenderOutput(context, 0).AsBlock()}";
            var subtract = RenderOutput(context, 1);
            var after = RenderOutput(context, 2);
            return $"difference(){(first + "\n" + subtract).AsBlock()}\n{after}";
        }
    }
}