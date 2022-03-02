using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class LessThanOperator : BinaryBooleanOperator
    {
        public override string NodeTitle => "<";
        public override string NodeDescription => "Compares if the first operand is\nless than the second operand.";
        protected override string OperatorSign => "<";
    }
}