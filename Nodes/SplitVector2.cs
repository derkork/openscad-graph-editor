using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Node for splitting a Vector2 into its components.
    /// </summary>
    [UsedImplicitly]
    public class SplitVector2 : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Split Vector2";
        public override string NodeDescription => "Splits a Vector2 into its components.";

        public override string NodeQuickLookup => "SV2";

        public SplitVector2()
        {
            InputPorts
                .Vector2(allowLiteral: false);

            OutputPorts
                .Number(allowLiteral:false)
                .Number(allowLiteral:false);

        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The Vector2 to split.";
                case 0 when portId.IsOutput:
                    return "The X component of the Vector2.";
                case 1 when portId.IsOutput:
                    return "The Y component of the Vector2.";
                default:
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var input = RenderInput(context, 0);
            switch (portIndex)
            {
                case 0:
                    return $"{input}.x";
                case 1:
                    return $"{input}.y";
                default:
                    return "";
            }
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.Vector2SplitIcon;
    }
}