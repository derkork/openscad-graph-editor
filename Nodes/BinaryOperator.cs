using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryOperator : ScadExpressionNode
    {
        
        protected abstract string OperatorSign { get; }

        public override string Render(IScadGraph context)
        {
            var left = RenderInput(context, 0);
            var right = RenderInput(context, 1);

            return $"({left} {OperatorSign} {right})";
        }
    }
}