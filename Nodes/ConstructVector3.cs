using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

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

        public override string Render(IScadGraph context)
        {
            return $"[{RenderInput(context, 0).OrDefault("0")}, {RenderInput(context, 1).OrDefault("0")}, {RenderInput(context, 2).OrDefault("0")}]";
        }
    }
}