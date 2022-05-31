using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A node representing a literal string.
    /// </summary>
    [UsedImplicitly]
    public class LiteralString  :ScadNode, IAmAnExpression, IHaveCustomWidget
    {
        public override string NodeTitle => "String";
        public override string NodeDescription => "A string.";

        public LiteralString()
        {
            OutputPorts
                .String();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            return "The string.";
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            return RenderLiteral(PortId.Output(portIndex));
        }

        public ScadNodeWidget InstantiateCustomWidget() => new SmallNodeWidget();
    }
}