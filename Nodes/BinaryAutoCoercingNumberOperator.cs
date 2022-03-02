namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryAutoCoercingNumberOperator : BinaryOperator
    {
        protected BinaryAutoCoercingNumberOperator()
        {
            InputPorts
                .Number(autoCoerce: true)
                .Number(autoCoerce: true);
            
            OutputPorts
                .Any();
        }
    }
}