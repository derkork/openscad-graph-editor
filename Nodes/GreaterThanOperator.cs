using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class GreaterThanOperator : BinaryComparisonOperator
    {
        public override string NodeTitle => ">";
        public override string NodeQuickLookup => "gta";
        public override string NodeDescription => "Compares if the first operand is greater than the second operand.";
        protected override string OperatorSign => ">";

        public override bool Supports(PortType portType)
        {
            // these only support numbers, strings, booleans and any (no vectors)
            return portType == PortType.Number || portType == PortType.String || portType == PortType.Boolean || portType == PortType.Any;
        }

        public override bool Supports(PortType first, PortType second, out PortType resultPortType)
        {
            // all combinations are supported as long as they are the same type and both are a supported type
            if (first != second)
            {
                resultPortType = PortType.Any;
                return false;
            }
            
            if (!Supports(first) || !Supports(second))
            {
                resultPortType = PortType.Any;
                return false;
            }

            resultPortType = PortType.Boolean;
            return true;
        }

        public override Texture NodeBackground => Resources.GreaterIcon;
    }
}