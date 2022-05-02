namespace OpenScadGraphEditor.Nodes
{
    public abstract class BooleanOperator : BinaryOperator
    {

        protected BooleanOperator()
        {
            InputPorts
                .Boolean(allowLiteral:false)
                .Boolean(allowLiteral:false);
            
            OutputPorts
                .Boolean(allowLiteral:false);
        }
    }
}