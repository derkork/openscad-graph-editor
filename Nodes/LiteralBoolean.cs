using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A node representing a literal string.
    /// </summary>
    [UsedImplicitly]
    public class LiteralBoolean  :ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Boolean";
        public override string NodeDescription => "A boolean.";

        public LiteralBoolean()
        {
            OutputPorts
                .Boolean();
        }

        public override string Render(IScadGraph context)
        {
            return RenderOutput(context, 0);
        }
    }
}