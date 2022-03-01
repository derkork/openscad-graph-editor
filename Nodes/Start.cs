using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Start : ScadNode
    {
        public override string NodeTitle => "Start";
        public override string NodeDescription => "Start of the code.";

        public override void _Ready()
        {
            OutputPorts
                .Flow("Start");
            
            base._Ready();
        }

        public override string Render(ScadContext scadContext)
        {
            return $@"/* created with OpenScadGraphEditor */
{RenderOutput(scadContext, 0)}
";
        }
    }
}