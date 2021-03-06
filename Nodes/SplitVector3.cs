using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Node for splitting a Vector3 into its components.
    /// </summary>
    [UsedImplicitly]
    public class SplitVector3 : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Split Vector3";
        public override string NodeDescription => "Splits a Vector3 into its components.";
        public override string NodeQuickLookup => "SV3";

        public SplitVector3()
        {
            InputPorts
                .Vector3(allowLiteral: false);
                
            OutputPorts
                .Number(allowLiteral: false)
                .Number(allowLiteral: false)
                .Number(allowLiteral: false);

        }
        
        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The Vector3 to split.";
                case 0 when portId.IsOutput:
                    return "The X component of the Vector3.";
                case 1 when portId.IsOutput:
                    return "The Y component of the Vector3.";
                case 2 when portId.IsOutput:
                    return "The Z component of the Vector3.";
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
                case 2:
                    return $"{input}.z";
                default:
                    return "";
            }
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.Vector3SplitIcon;
    }
}