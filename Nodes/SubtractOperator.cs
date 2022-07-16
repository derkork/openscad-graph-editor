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

        public override Texture NodeBackground => Resources.MinusIcon;
    }
}