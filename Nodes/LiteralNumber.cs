using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A node representing a literal number.
    /// </summary>
    [UsedImplicitly]
    public class LiteralNumber  :ScadNode, IAmAnExpression, IHaveCustomWidget
    {
        public override string NodeTitle => "Number";
        public override string NodeQuickLookup => "Nbr";
        public override string NodeDescription => "A number.";

        public LiteralNumber()
        {
            OutputPorts
                .Number();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            return "The number.";
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            return RenderLiteral(PortId.Output(portIndex));
        }

        public ScadNodeWidget InstantiateCustomWidget() => new SmallNodeWidget();
    }
}