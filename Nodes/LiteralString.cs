using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// A node representing a literal string.
    /// </summary>
    public class LiteralString  :ScadExpressionNode
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
    }
}