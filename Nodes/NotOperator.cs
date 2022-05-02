using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Operator for negating a boolean value.
    /// </summary>
    [UsedImplicitly]
    public class NotOperator : ScadNode, IAmAnExpression, IHaveCustomWidget,IHaveNodeBackground
    {
        public override string NodeTitle => "Not";
        public override string NodeDescription => "Boolean NOT (!)";


        public NotOperator()
        {
            InputPorts
                .Boolean(allowLiteral:false);

            OutputPorts
                .Boolean(allowLiteral:false);
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.NotIcon;

        public override string Render(IScadGraph context)
        {
            var value = RenderInput(context, 0);
            return value.Empty() ? "" : $"!({value})";
        }
    }
}