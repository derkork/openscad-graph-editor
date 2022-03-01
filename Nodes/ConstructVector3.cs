using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ConstructVector3 : ScadExpressionNode
    {
        public override string NodeTitle => "Construct Vector3";
        public override string NodeDescription => "Constructs a Vector3 from its components.";

        public override void _Ready()
        {
            InputPorts
                .Number("X")
                .Number("Y")
                .Number("Z");

            OutputPorts
                .Vector3(allowLiteral: false);

            base._Ready();
        }

        public override string Render(ScadContext scadContext)
        {
            return $"[{RenderInput(scadContext, 0)}, {RenderInput(scadContext, 1)}, {RenderInput(scadContext, 2)}]";
        }
    }
}