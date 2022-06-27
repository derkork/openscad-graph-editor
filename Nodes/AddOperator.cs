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
                   || portType == PortType.Array
                   || portType == PortType.Any;
        }

        public override Texture NodeBackground => Resources.PlusIcon;
    }
}