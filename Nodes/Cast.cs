using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class Cast : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Cast";
        public override string NodeDescription => "Allows you to cast a value to another type.";
        public override string NodeQuickLookup => "Cst";

        public Cast()
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
                    return "The value to cast.";
                case 0 when portId.IsOutput:
                    return "The casted value.";
                default:
                    return "";
            }
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            // openscad doesn't really have casts, so we simply output the input, the casting is just for the editor.
            return RenderInput(context, 0);
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.CastIcon;
    }
}