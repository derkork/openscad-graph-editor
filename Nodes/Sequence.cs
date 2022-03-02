using System.Linq;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public class Sequence : ScadNode
    {
        public override string NodeTitle => "Sequence";
        public override string NodeDescription => "Runs its right ports in order";

        public Sequence()
        {
            InputPorts
                .Flow();
            
            OutputPorts
                .Flow()
                .Flow()
                .Flow()
                .Flow();
        }

        public override string Render(ScadContext scadContext)
        {
            return "".AppendLines(
                OutputPorts.Indices()
                    .Select(it => RenderOutput(scadContext, it))
                    .ToArray()
            );
        }
    }
}