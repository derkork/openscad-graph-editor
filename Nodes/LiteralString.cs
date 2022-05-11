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

        public override string Render(IScadGraph context)
        {
            return RenderOutput(context, 0);
        }

        public ScadNodeWidget InstantiateCustomWidget() => new SmallNodeWidget();
    }
}