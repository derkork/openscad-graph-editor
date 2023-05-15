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

        public override bool Supports(PortType first, PortType second, out PortType resultPortType)
        {
            // if any port type is not supported, the result is ANY
            if (!Supports(first) || !Supports(second))
            {
                resultPortType = PortType.Any;
                return false;
            }
            
            // if any of the port types is ANY, the result is ANY
            if (first == PortType.Any || second == PortType.Any)
            {
                resultPortType = PortType.Any;
                return true;
            }
            
            // if the divisor is a number, the result is the port type of the dividend
            if (second == PortType.Number)
            {
                resultPortType =  first;
                return true;
            }
            
            // if the dividend is a number, the result is the port type of the divisor
            if (first == PortType.Number)
            {
                resultPortType =  second;
                return true;
            };
            
            resultPortType = PortType.Any;
            return false;
        }

        protected override PortType CalculateOutputPortType()
        {
            if (Supports(GetPortType(PortId.Input(0)),GetPortType(PortId.Input(1)), out var result))
            {
                return result;
            }
            
            // every other case is not supported, so ANY
            return PortType.Any;
        }

        public override Texture NodeBackground => Resources.DivideIcon;
    }
}