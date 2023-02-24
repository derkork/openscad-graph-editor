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
    public class Half : ScadNode, IAmAnExpression, IHaveNodeBackground, IHaveCustomWidget
    {
        public override string NodeTitle => "Half";
        public override string NodeQuickLookup => "Hlf";
        public override string NodeDescription => "Returns half of the input.";


        public Half()
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
                    return "Half of the input value.";
                default: 
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var value = RenderInput(context, 0);
            return value.Empty() ? "" : $"({value} / 2)";
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.HalfIcon;
    }
}