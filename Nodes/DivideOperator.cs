using JetBrains.Annotations;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class DivideOperator : BinaryAutoCoercingNumberOperator
    {
        public override string NodeTitle => "/";
        public override string NodeDescription => "Divides the given inputs.";
        protected override string OperatorSign => "/";
    }
}