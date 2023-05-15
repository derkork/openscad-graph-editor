using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class NotEqualOperator : BinaryComparisonOperator
    {
        public override string NodeTitle => "!=";
        public override string NodeQuickLookup => "!=";
        public override string NodeDescription => "Compares if two values are not equal.";
        protected override string OperatorSign => "!=";

        public override bool Supports(PortType portType)
        {
            return portType == PortType.Number 
                   || portType == PortType.String 
                   || portType == PortType.Boolean 
                   || portType == PortType.Any 
                   || portType == PortType.Vector2 
                   || portType == PortType.Vector3 
                   || portType == PortType.Vector;
        }


        public override bool Supports(PortType first, PortType second, out PortType resultPortType)
        {
            // all combinations are supported and result in a boolean
            if (!Supports(first) || !Supports(second))
            {
                resultPortType = PortType.Any;
                return false;
            }
            
            resultPortType = PortType.Boolean;
            return true;
        }

        public override Texture NodeBackground => Resources.LessIcon;
    }
}