using System.Linq;
using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class MultiplyOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        public override string NodeTitle => "*";
        public override string NodeQuickLookup => "**";
        public override string NodeDescription => "Multiplies the given inputs.";
        protected override string OperatorSign => "*";


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
            var portTypes = new[] {first, second};
            
            // if any port type is not supported, the result is ANY
            if (portTypes.Any(it => !Supports(it)))
            {
                resultPortType = PortType.Any;
                return false;
            }
            
            // if any of the port types is ANY, the result is ANY
            if (portTypes.Any(it => it == PortType.Any))
            {
                resultPortType = PortType.Any;
                return true;
            }
            
            // if both are numbers, the result is a number
            if (portTypes.All(it => it == PortType.Number))
            {
                resultPortType =  PortType.Number;
                return true;
            }
            
            // if both are vector types the result is a number (dot product)
            if (portTypes.All(it => it == PortType.Vector2 || it == PortType.Vector3 || it == PortType.Vector))
            {
                resultPortType =  PortType.Number;
                return true;
            }
            
            // if one factor is a number and the other one is a vector type (vector, vector2, vector3), the result is the vector type
            if (portTypes.Any(it => it == PortType.Number) && portTypes.Any(it => it == PortType.Vector2 || it == PortType.Vector3 || it == PortType.Vector))
            {
                resultPortType =  portTypes.First(it => it == PortType.Vector2 || it == PortType.Vector3 || it == PortType.Vector);
                return true;
            }
            
            // should never happen, but if we get here, it's ANY
            resultPortType =  PortType.Any;
            return false;
        }

        protected override PortType CalculateOutputPortType()
        {
            if (Supports(GetPortType(PortId.Input(0)), GetPortType(PortId.Input(1)), out var result))
            {
                return result;
            }

            return PortType.Any;
        }

        public override Texture NodeBackground => Resources.TimesIcon;
    }
}