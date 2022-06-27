using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Child : ScadNode, ICanHaveModifier, IImplyChildren
    {
        public override string NodeTitle => "Child";
        public override string NodeDescription => "Renders a single child of a module.";

        public override string NodeQuickLookup => "1Cld";

        public Child()
        {
            InputPorts
                .Number("Index");

            OutputPorts
                .Geometry();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The index of the child to render.";
                case 0 when portId.IsOutput:
                    return "The geometry of the child with the given index.";
                default:
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var index = RenderInput(context, 0);
            return $"children({index});";
        }
    }
}