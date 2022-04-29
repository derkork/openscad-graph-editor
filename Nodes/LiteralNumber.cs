using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A node representing a literal number.
    /// </summary>
    [UsedImplicitly]
    public class LiteralNumber  :ScadNode, IAmAnExpression
    {
        public override string NodeTitle => "Number";
        public override string NodeDescription => "A number.";

        public LiteralNumber()
        {
            OutputPorts
                .Number();
        }

        public override string Render(IScadGraph context)
        {
            var value = RenderOutput(context, 0);
            return $"{value}";
        }
    }
}