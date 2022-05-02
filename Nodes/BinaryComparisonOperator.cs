namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryComparisonOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        protected BinaryComparisonOperator()
        {
            OutputPorts.Clear();
            OutputPorts
                .Boolean(allowLiteral: false);
        }
    }
}