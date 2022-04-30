using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ConstructVector3 : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Construct Vector3";
        public override string NodeDescription => "Constructs a Vector3 from its components.";

        public ConstructVector3()
        {
            InputPorts
                .Number()
                .Number()
                .Number();

            OutputPorts
                .Vector3(allowLiteral: false);
        }

        public override string Render(IScadGraph context)
        {
            return $"[{RenderInput(context, 0).OrDefault("0")}, {RenderInput(context, 1).OrDefault("0")}, {RenderInput(context, 2).OrDefault("0")}]";
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.Vector3MergeIcon;
    }
}