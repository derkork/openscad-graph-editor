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

        protected override PortType CalculateOutputPortType()
        {
            // all comparison operators always return a boolean
            return PortType.Boolean;
        }
    }
}