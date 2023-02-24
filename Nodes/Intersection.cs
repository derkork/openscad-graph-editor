using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Intersection : ScadNode, ICanHaveModifier
    {
        public override string NodeTitle => "Intersection";
        public override string NodeQuickLookup => "Irt";

        public override string NodeDescription =>
            "Returns the geometry that all inputs have in common (logical AND).";

        public Intersection()
        {
            InputPorts
                .Geometry();

            OutputPorts
                .Geometry();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The geometry that should be intersected.";
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
            
            return $"intersection(){RenderInput(context, 0).AsBlock()}";
        }
    }
}