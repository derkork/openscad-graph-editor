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

        protected override PortType CalculateOutputPortType()
        {
            var firstPortType = GetPortType(PortId.Input(0));
            var secondPortType = GetPortType(PortId.Input(1));
            var portTypes = new[] {firstPortType, secondPortType};
            
            // if any of the port types is ANY, the result is ANY
            if (portTypes.Any(it => it == PortType.Any))
            {
                return PortType.Any;
            }
            
            // only subtractions between numbers and vectors are supported but not a mix of numbers or vectors
            if (portTypes.All(it => it == PortType.Number))
            {
                return PortType.Number;
            }
            
            // if we subtract vector types the result is the smallest vector format we can find
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
            
            // should not happen but just in case...
            return PortType.Any;
        }

        public override Texture NodeBackground => Resources.MinusIcon;
    }
}