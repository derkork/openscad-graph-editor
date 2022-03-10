using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ConstructVector3 : ScadExpressionNode
    {
        public override string NodeTitle => "Construct Vector3";
        public override string NodeDescription => "Constructs a Vector3 from its components.";

        public ConstructVector3()
        {
            InputPorts
                .Number("X")
                .Number("Y")
                .Number("Z");

            OutputPorts
                .Vector3(allowLiteral: false);
        }

        public override string Render(ScadInvokableContext scadInvokableContext)
        {
            return $"[{RenderInput(scadInvokableContext, 0)}, {RenderInput(scadInvokableContext, 1)}, {RenderInput(scadInvokableContext, 2)}]";
        }
    }
}