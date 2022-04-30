using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class LessEqualOperator : BinaryBooleanOperator
    {
        public override string NodeTitle => "<=";
        public override string NodeDescription => "Compares if the first operand is\nless than or equal to the second operand.";
        protected override string OperatorSign => "<=";

        public override Texture NodeBackground => Resources.LessEqualIcon;
    }
}