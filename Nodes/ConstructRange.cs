using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ConstructRange : ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Construct Range";
        public override string NodeQuickLookup => "CRng";
        public override string NodeDescription => "Constructs a range of numbers. All input parameters can also be rational numbers.";


        public ConstructRange()
        {
            InputPorts
                .Number("Start")
                .Number("Step", defaultValue: 1)
                .Number("End");

            OutputPorts
                .Array();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The start of the range.";
                case 1 when portId.IsInput:
                    return "The step size of the range.";
                case 2 when portId.IsInput:
                    return "The end of the range. This is the largest number to be included in the range.";
                case 0 when portId.IsOutput:
                    return "The range of numbers.";
                default:
                    return "";
            }
        }


        public override string Render(ScadGraph context, int portIndex)
        {
            var start = RenderInput(context, 0).OrDefault("0");
            var step = RenderInput(context, 1).OrDefault("1");
            var end = RenderInput(context, 2).OrDefault("1");

            return $"[{start}:{step}:{end}]";
        }

    }
}