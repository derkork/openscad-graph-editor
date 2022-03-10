using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryOperator : ScadExpressionNode
    {
        
        protected abstract string OperatorSign { get; }

        public override string Render(ScadInvokableContext scadInvokableContext)
        {
            var left = RenderInput(scadInvokableContext, 0);
            var right = RenderInput(scadInvokableContext, 1);

            return $"({left} {OperatorSign} {right})";
        }
    }
}