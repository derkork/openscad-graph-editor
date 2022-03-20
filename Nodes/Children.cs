using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Children : ScadNode
    {
        public override string NodeTitle => "Children";
        public override string NodeDescription => "Renders all children or a subset of all children of a module.";

        public Children()
        {
            InputPorts
                .Flow()
                .Array("Selection");

            OutputPorts
                .Flow();
        }
        
        public override string Render(IScadGraph context)
        {
            var subset = RenderInput(context, 1);
            return $"children({subset});";
        }
    }
}