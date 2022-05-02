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

        public Cast()
        {
            InputPorts
                .Any();

            OutputPorts
                .Any();
        }
        
        public override string Render(IScadGraph context)
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