using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class GreaterEqualOperator : BinaryComparisonOperator
    {
        public override string NodeTitle => ">=";
        public override string NodeQuickLookup => "gte";
        public override string NodeDescription => "Compares if the first operand is greater than or equal to the second operand.";
        protected override string OperatorSign => ">=";

        public override bool Supports(PortType portType)
        {
            // these only support numbers, strings, booleans and any (no vectors)
            return portType == PortType.Number || portType == PortType.String || portType == PortType.Boolean || portType == PortType.Any;
        }


        public override Texture NodeBackground => Resources.GreaterEqualIcon;
    }
}