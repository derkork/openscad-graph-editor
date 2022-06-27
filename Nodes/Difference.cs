using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Difference : ScadNode, ICanHaveModifier
    {
        public override string NodeTitle => "Difference";
        public override string NodeQuickLookup => "Dfr";

        public override string NodeDescription =>
            "Subtracts the child nodes from the first one (logical and not).";

        public Difference()
        {
            InputPorts
                .Geometry("Add")
                .Geometry("Subtract");

            OutputPorts
                .Geometry();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The geometry from which the other geometry should be subtracted.";
                case 1 when portId.IsInput:
                    return "The geometry that should be subtracted from the first geometry.";
                case 0 when portId.IsOutput:
                    return "The result geometry.";
                default:
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex != 0)
            {
                return "";
            }
            
            var first = $"union(){RenderInput(context, 0).AsBlock()}";
            var subtract = RenderInput(context, 1);
            return $"difference(){(first + "\n" + subtract).AsBlock()}";
        }
    }
}