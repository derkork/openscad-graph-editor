using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Operation for calculating the double of a value.
    /// </summary>
    [UsedImplicitly]
    public class Double : ScadNode, IAmAnExpression, IHaveNodeBackground, IHaveCustomWidget
    {
        public override string NodeTitle => "Double";
        public override string NodeQuickLookup => "Dbl";
        public override string NodeDescription => "Returns double of the input.";


        public Double()
        {
            InputPorts
                .Any();

            OutputPorts
                .Any();
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The input value.";
                case 0 when portId.IsOutput:
                    return "Double of the input value.";
                default: 
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var value = RenderInput(context, 0);
            return value.Empty() ? "" : $"({value} * 2)";
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }
        
        public Texture NodeBackground => Resources.DoubleIcon;
    }
}