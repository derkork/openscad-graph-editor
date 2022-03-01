using System.Collections.Generic;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class RotateAxisAngle : ScadNode
    {
        public override string NodeTitle => "Rotate (Axis/Angle)";

        public override string NodeDescription =>
            "Rotates the next elements\nalong the given axis and angle.";

        public override void _Ready()
        {
            InputPorts
                .Flow()
                .Vector3("Axis")
                .Number("Angle");

            OutputPorts
                .Flow();
            
            base._Ready();
        }

        public override string Render(ScadContext scadContext)
        {
            var axis = RenderInput(scadContext, 1);
            var angle = RenderInput(scadContext, 2);
            var next = RenderOutput(scadContext, 0);
            if (next.Length == 0)
            {
                return "";
            }

            return $"rotate(a={angle}, v={axis}) {{"
                .AppendLines(
                    next.Indent(),
                    "}"
                );
        }
    }
}