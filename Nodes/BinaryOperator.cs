using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryOperator : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        
        protected abstract string OperatorSign { get; }

        public override string Render(IScadGraph context)
        {
            var left = RenderInput(context, 0);
            var right = RenderInput(context, 1);

            return $"({left.OrUndef()} {OperatorSign} {right.OrUndef()})";
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public abstract Texture NodeBackground { get; }
    }
}