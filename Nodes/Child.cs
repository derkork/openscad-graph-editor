using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Child : ScadNode, ICanHaveModifier, IImplyChildren
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
            var next = RenderOutput(context, 0);
            return $"children({index});\n{next}";
        }
    }
}