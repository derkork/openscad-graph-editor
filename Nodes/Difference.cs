using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class Difference : ScadNode
    {
        public override string NodeTitle => "Difference";

        public override string NodeDescription =>
            "Subtracts the 2nd (and all further) child nodes from the first one (logical and not).";

        public Difference()
        {
            InputPorts
                .Flow();

            OutputPorts
                .Flow("Input")
                .Flow("Subtract")
                .Flow("After");
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