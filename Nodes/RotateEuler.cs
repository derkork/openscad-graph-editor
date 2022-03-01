using System.Collections.Generic;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class RotateEuler : ScadNode
    {
        public override string NodeTitle => "Rotate (Euler angles)";

        public override string NodeDescription =>
            "Rotates the next elements\nalong the given Euler angles.";

        public override void _Ready()
        {
            InputPorts
                .Flow()
                .Vector3("Angles");

            OutputPorts
                .Flow();
            
            base._Ready();
        }

        public override string Render(ScadContext scadContext)
        {
            var angles = RenderInput(scadContext, 1);
            var next = RenderOutput(scadContext, 0);
            if (next.Length == 0)
            {
                return "";
            }

            return $"rotate(a={angles}) {{"
                .AppendLines(
                    next.Indent(),
                    "}"
                );
        }
    }
}