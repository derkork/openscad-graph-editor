using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Operation for calculating the value minus 1.
    /// </summary>
    [UsedImplicitly]
    public class MinusOne : ScadNode, IAmAnExpression, IHaveNodeBackground, IHaveCustomWidget
    {
        public override string NodeTitle => "-1";
        public override string NodeQuickLookup => "-1";
        public override string NodeDescription => "Returns the input minus 1.";


        public MinusOne()
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
                    return "The input value minus 1.";
                default: 
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            var value = RenderInput(context, 0);
            return value.Empty() ? "" : $"({value} - 1)";
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }
        public Texture NodeBackground => Resources.MinusOneIcon;
    }
}