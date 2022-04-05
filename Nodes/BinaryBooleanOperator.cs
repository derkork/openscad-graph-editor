namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryBooleanOperator : BinaryOperator
    {
        protected BinaryBooleanOperator()
        {
            OutputPorts
                .Boolean(allowLiteral: false);
        }
    }
}