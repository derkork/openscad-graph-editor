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
            
            // if both are numbers, the result is a number
            if (portTypes.All(it => it == PortType.Number))
            {
                return PortType.Number;
            }
            
            // if both are vector types the result is a number (dot product)
            if (portTypes.All(it => it == PortType.Vector2 || it == PortType.Vector3 || it == PortType.Vector))
            {
                return PortType.Number;
            }
            
            // if one factor is a number and the other one is a vector type (vector, vector2, vector3), the result is the vector type
            if (portTypes.Any(it => it == PortType.Number) && portTypes.Any(it => it == PortType.Vector2 || it == PortType.Vector3 || it == PortType.Vector))
            {
                return portTypes.First(it => it == PortType.Vector2 || it == PortType.Vector3 || it == PortType.Vector);
            }
            
            // should never happen, but if we get here, it's ANY
            return PortType.Any;
            
        }

        public override Texture NodeBackground => Resources.TimesIcon;
    }
}