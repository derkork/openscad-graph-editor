using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class OrOperator : BooleanOperator
    {
        public override string NodeTitle => "Or";
        public override string NodeQuickLookup => "|";
        public override string NodeDescription => "Boolean OR (||)";
        protected override string OperatorSign => "||";
        public override Texture NodeBackground => Resources.OrIcon;
    }
}