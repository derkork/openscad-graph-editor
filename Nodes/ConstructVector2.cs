using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ConstructVector2 : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Construct Vector2";
        public override string NodeDescription => "Constructs a Vector2 from its components.";

        public ConstructVector2()
        {
            InputPorts
                .Number()
                .Number();

            OutputPorts
                .Vector2(allowLiteral: false);
        }

        public override string Render(IScadGraph context)
        {
            return $"[{RenderInput(context, 0).OrDefault("0")}, {RenderInput(context, 1).OrDefault("0")}]";
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.Vector2MergeIcon;
    }
}