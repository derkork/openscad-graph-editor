using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class MultiplyOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        public override string NodeTitle => "*";
        public override string NodeDescription => "Multiplies the given inputs.";
        protected override string OperatorSign => "*";

        public override Texture NodeBackground => Resources.TimesIcon;
    }
}