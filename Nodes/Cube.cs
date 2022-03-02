using System.Collections.Generic;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Cube : ScadNode
    {
    public override string NodeTitle => "Cube";

        public override string NodeDescription =>
            "Creates a cube in the first octant. When center is true, the cube is centered on the origin.";

        public Cube()
        {
            InputPorts
                .Flow()
                .Vector3("Size")
                .Boolean("Center");

            OutputPorts
                .Flow();
        }

        public override string Render(ScadContext scadContext)
        {
            var position = RenderInput(scadContext, 1);
            var center = RenderInput(scadContext, 2);
            var next = RenderOutput(scadContext, 0);
            return $"cube({position}, {center});"
                .AppendLines(next);
        }
    }
}