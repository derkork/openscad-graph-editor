using System.Collections.Generic;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public sealed class Start : ScadNode, IGraphEntryPoint
    {
        public override string NodeTitle => "Start";
        public override string NodeDescription => "Start of the code.";


        public Start()
        {
            OutputPorts
                .Flow("Start");
        }

        public override string Render(ScadInvokableContext scadInvokableContext)
        {
            return $@"/* created with OpenScadGraphEditor */
{RenderOutput(scadInvokableContext, 0)}
";
        }
    }
}