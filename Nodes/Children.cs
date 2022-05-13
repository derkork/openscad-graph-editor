using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Children : ScadNode, ICanHaveModifier, IImplyChildren
    {
        public override string NodeTitle => "Children";
        public override string NodeDescription => "Renders all children or a subset of all children of a module.";

        public Children()
        {
            InputPorts
                .Flow()
                .Array("Selection");

            OutputPorts
                .Flow();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "Input flow";
                case 1 when portId.IsInput:
                    return "A vector with indices of the children to render. If not given, all children will be rendered.";
                case 0 when portId.IsOutput:
                    return "Output flow";
                default:
                    return "";
            }
        }


        public override string Render(IScadGraph context)
        {
            var subset = RenderInput(context, 1);
            var next = RenderOutput(context, 0);
            return $"children({subset});\n{next}";
        }
    }
}