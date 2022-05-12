using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    public abstract class BinaryOperator : ScadNode, IAmAnExpression, IHaveCustomWidget, IHaveNodeBackground
    {
        
        protected abstract string OperatorSign { get; }

        public override string Render(IScadGraph context)
        {
            var left = RenderInput(context, 0);
            var right = RenderInput(context, 1);

            return $"({left.OrUndef()} {OperatorSign} {right.OrUndef()})";
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                switch (portId.Port)
                {
                    case 0:
                        return "The first operand.";
                    case 1:
                        return "The second operand.";
                }
            }

            if (portId.IsOutput)
            {
                if (portId.Port == 0)
                {
                    return "The result of the operation.";
                }
            }

            return "";
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public abstract Texture NodeBackground { get; }
    }
}