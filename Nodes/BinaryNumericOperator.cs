namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryNumericOperator : ScadExpressionNode
    {
        
        protected abstract string OperatorSign { get; }
        
        public override void _Ready()
        {
            InputPorts
                .Number()
                .Number();

            OutputPorts
                .Number(allowLiteral: false);
            base._Ready();
        }

        public override string Render(ScadContext scadContext)
        {
            var left = RenderInput(scadContext, 0);
            var right = RenderInput(scadContext, 1);

            return $"({left} {OperatorSign} {right})";
        }
    }
}