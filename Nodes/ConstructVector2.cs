using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes.ConstructVector;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ConstructVector2 : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground, IAmAVectorConstruction
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

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "X component of the vector";
                case 1 when portId.IsInput:
                    return "Y component of the vector";
                case 0 when portId.IsOutput:
                    return "The resulting Vector2.";
                default:
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
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