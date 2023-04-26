using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class DivideOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        public override string NodeTitle => "/";
        public override string NodeQuickLookup => "//";
        public override string NodeDescription => "Divides the given inputs.";
        protected override string OperatorSign => "/";

        public override bool Supports(PortType portType)
        {
            return portType == PortType.Number
                   || portType == PortType.Vector2
                   || portType == PortType.Vector3 
                   || portType == PortType.Vector
                   || portType == PortType.Any;
            
        }

        protected override PortType CalculateOutputPortType()
        {
            var firstPortType = GetPortType(PortId.Input(0));
            var secondPortType = GetPortType(PortId.Input(1));
            
            // if any of the port types is ANY, the result is ANY
            if (firstPortType == PortType.Any || secondPortType == PortType.Any)
            {
                return PortType.Any;
            }
            
            // if the divisor is a number, the result is the port type of the dividend
            if (secondPortType == PortType.Number)
            {
                return firstPortType;
            }
            
            // if the dividend is a number, the result is the port type of the divisor
            if (firstPortType == PortType.Number)
            {
                return secondPortType;
            }
            
            // every other case is not supported, so ANY
            return PortType.Any;
        }

        public override Texture NodeBackground => Resources.DivideIcon;
    }
}