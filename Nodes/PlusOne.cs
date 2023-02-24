using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Operation for calculating the half of a value.
    /// </summary>
    [UsedImplicitly]
    public class PlusOne : ScadNode, IAmAnExpression, IHaveNodeBackground, IHaveCustomWidget
    {
        public override string NodeTitle => "+1";
        public override string NodeQuickLookup => "+1";
        public override string NodeDescription => "Returns the input plus 1.";


        public PlusOne()
        {
            InputPorts
                .Number();

            OutputPorts
                .Number(allowLiteral: false);
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The input value.";
                case 0 when portId.IsOutput:
                    return "The input value plus 1.";
                default: 
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var value = RenderInput(context, 0);
            return value.Empty() ? "" : $"({value} + 1)";
        }
        
        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }
        
        public Texture NodeBackground => Resources.PlusOneIcon;
    }
}