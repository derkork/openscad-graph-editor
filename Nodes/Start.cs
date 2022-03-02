using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public sealed class Start : ScadNode
    {
        public override string NodeTitle => "Start";
        public override string NodeDescription => "Start of the code.";


        public Start()
        {
            OutputPorts
                .Flow("Start");
        }

        public override string Render(ScadContext scadContext)
        {
            return $@"/* created with OpenScadGraphEditor */
{RenderOutput(scadContext, 0)}
";
        }
    }
}