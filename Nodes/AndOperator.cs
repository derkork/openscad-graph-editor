using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class AndOperator : BooleanOperator
    {
        public override string NodeTitle => "And";
        public override string NodeDescription => "Boolean AND (&&)";
        protected override string OperatorSign => "&&";
        public override Texture NodeBackground => Resources.AndIcon;
    }
}