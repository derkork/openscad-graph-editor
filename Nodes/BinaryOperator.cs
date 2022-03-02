namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryOperator : ScadExpressionNode
    {
        
        protected abstract string OperatorSign { get; }

        public override string Render(ScadContext scadContext)
        {
            var left = RenderInput(scadContext, 0);
            var right = RenderInput(scadContext, 1);

            return $"({left} {OperatorSign} {right})";
        }
    }
}