using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryOperator : ScadNode, IAmAnExpression
    {
        
        protected abstract string OperatorSign { get; }

        public override string Render(IScadGraph context)
        {
            var left = RenderInput(context, 0);
            var right = RenderInput(context, 1);

            return $"(({left.OrUndef()}) {OperatorSign} ({right.OrUndef()}))";
        }
    }
}