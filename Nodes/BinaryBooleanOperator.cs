namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryBooleanOperator : BinaryOperator
    {
        protected BinaryBooleanOperator() : base()
        {
            OutputPorts
                .Boolean();
        }
    }
}