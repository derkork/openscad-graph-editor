using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Translate : ScadNode
    {
        public override string NodeTitle => "Translate";

        public override string NodeDescription =>
            "Translates (moves) its child\nelements along the specified offset.";

        public override void _Ready()
        {
            InputPorts
                .Flow()
                .Vector3("Offset");

            OutputPorts
                .Flow();
            
            base._Ready();
        }

        public override string Render(ScadContext scadContext)
        {
            var offset = RenderInput(scadContext, 1);
            var next = RenderOutput(scadContext, 0);
            if (next.Length == 0)
            {
                return "";
            }

            return $"translate({offset}) {{"
                .AppendLines(
                    next.Indent(),
                    "}"
                );
        }
    }
}