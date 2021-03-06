using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A node representing a literal string.
    /// </summary>
    [UsedImplicitly]
    public class LiteralBoolean  :ScadNode, IAmAnExpression, IHaveCustomWidget
    {
        public override string NodeTitle => "Boolean";
        public override string NodeQuickLookup => "Bol";
        public override string NodeDescription => "A boolean.";

        public LiteralBoolean()
        {
            OutputPorts
                .Boolean();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            return "The boolean.";
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            return RenderLiteral(PortId.Output(portIndex));
        }

        public ScadNodeWidget InstantiateCustomWidget() => new SmallNodeWidget();
    }
}