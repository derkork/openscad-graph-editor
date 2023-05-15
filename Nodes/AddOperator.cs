using System.Linq;
using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class AddOperator : SwitchableBinaryOperator.SwitchableBinaryOperator
    {
        public override string NodeTitle => "+";
        public override string NodeDescription => "Adds the given inputs.";
        public override string NodeQuickLookup => "++";
        protected override string OperatorSign => "+";

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
            
            // all are numbers -> number
            if (portTypes.All(it => it == PortType.Number))
            {
                resultPortType =  PortType.Number;
                return true;
            }
            
            // numbers are only compatible with numbers, so if we still have a number in play, it's ANY
            if (portTypes.Any(it => it == PortType.Number))
            {
                resultPortType =  PortType.Any;
                return false;
            }
            
            // now only vectors are left, and in this case the result is the smallest vector format we can find
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
            
            // this shouldn't happen but just in case
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

        public override Texture NodeBackground => Resources.PlusIcon;
    }
}