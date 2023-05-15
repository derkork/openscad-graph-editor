using System.Linq;
using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class SubtractOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        public override string NodeTitle => "-";
        public override string NodeQuickLookup => "--";
        public override string NodeDescription => "Subtracts the given inputs.";
        protected override string OperatorSign => "-";


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
            
            // only subtractions between numbers and vectors are supported but not a mix of numbers or vectors
            if (portTypes.All(it => it == PortType.Number))
            {
                resultPortType = PortType.Number;
                return true;
            }
            
            // if we subtract vector types the result is the smallest vector format we can find
            // vector2 < vector3 < vector
            if (portTypes.Any(it => it == PortType.Vector2))
            {
                resultPortType =  PortType.Vector2;
                return true;
            }
            
            if (portTypes.Any(it => it == PortType.Vector3))
            {
                resultPortType =  PortType.Vector3;
                return true;
            }
            
            if (portTypes.Any(it => it == PortType.Vector))
            {
                resultPortType =  PortType.Vector;
                return true;
            }
            
            // should not happen but just in case...
            resultPortType =  PortType.Any;
            return false;
        }

        protected override PortType CalculateOutputPortType()
        {
            if (Supports(GetPortType(PortId.Input(0)), GetPortType(PortId.Input(1)), out var resultPortType))
            {
                return resultPortType;
            }

            return PortType.Any;
        }

        public override Texture NodeBackground => Resources.MinusIcon;
    }
}