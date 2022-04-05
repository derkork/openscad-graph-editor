using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Cast : ScadExpressionNode
    {
        public override string NodeTitle => "Cast";
        public override string NodeDescription => "Allows you to cast a value to another type.";

        public Cast()
        {
            InputPorts
                .Any();

            OutputPorts
                .Any();
        }
        
        public override string Render(IScadGraph context)
        {
            // openscad doesn't really have casts, so we simply output the input, the casting is just for the editor.
            return RenderInput(context, 0);
        }
    }
}