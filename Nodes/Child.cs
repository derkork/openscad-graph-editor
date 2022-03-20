using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Child : ScadNode
    {
        public override string NodeTitle => "Child";
        public override string NodeDescription => "Renders a single child of a module.";

        public Child()
        {
            InputPorts
                .Flow()
                .Number("Index");

            OutputPorts
                .Flow();
        }
        
        public override string Render(IScadGraph context)
        {
            var index = RenderInput(context, 1);
            return $"children({index});";
        }
    }
}