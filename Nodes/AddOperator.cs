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

        protected override PortType CalculateOutputPortType()
        {
            var portTypes = new[] {GetPortType(PortId.Input(0)), GetPortType(PortId.Input(1))};

            // if any of the port types is ANY, the result is ANY
            if (portTypes.Any(it => it == PortType.Any))
            {
                return PortType.Any;
            }
            
            // all are numbers -> number
            if (portTypes.All(it => it == PortType.Number))
            {
                return PortType.Number;
            }
            
            // numbers are only compatible with numbers, so if we still have a number in play, it's ANY
            if (portTypes.Any(it => it == PortType.Number))
            {
                return PortType.Any;
            }
            
            // now only vectors are left, and in this case the result is the smallest vector format we can find
            // vector2 < vector3 < vector
            if (portTypes.Any(it => it == PortType.Vector2))
            {
                return PortType.Vector2;
            }
            
            if (portTypes.Any(it => it == PortType.Vector3))
            {
                return PortType.Vector3;
            }
            
            if (portTypes.Any(it => it == PortType.Vector))
            {
                return PortType.Vector;
            }
            
            // this shouldn't happen but just in case
            return PortType.Any;
        }

        public override Texture NodeBackground => Resources.PlusIcon;
    }
}