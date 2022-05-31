using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryOperator : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        
        protected abstract string OperatorSign { get; }

        public override string Render(ScadGraph context, int portIndex)
        {
            var left = RenderInput(context, 0);
            var right = RenderInput(context, 1);

            return $"({left.OrUndef()} {OperatorSign} {right.OrUndef()})";
        }

        public override string GetPortDocumentation(PortId portId)
        {
            switch (portId.Port)
            {
                case 0 when portId.IsInput:
                    return "The first operand.";
                case 1 when portId.IsInput:
                    return "The second operand.";
                case 0 when portId.IsOutput:
                    return "The result of the operation.";
                default:
                    return "";
            }
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public abstract Texture NodeBackground { get; }
    }
}