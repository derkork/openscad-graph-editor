using System;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class SplitVector3 : ScadExpressionNode, IMultiExpressionOutputNode
    {
        public override string NodeTitle => "Split Vector3";
        public override string NodeDescription => "Splits a Vector3 into its components.";

        public SplitVector3()
        {
            InputPorts
                .Vector3(allowLiteral: false);
                
            OutputPorts
                .Number("X", false)
                .Number("Y", false)
                .Number("Z", false);

        }

        public override string Render(IScadGraph context)
        {
            GdAssert.That(false, "This node cannot render.");
            return "";
        }

        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            var input = RenderInput(context, 0);
            GdAssert.That(port >= 0 && port < 3, "port out of range");
            switch (port)
            {
                case 0:
                    return $"({input}).x";
                case 1:
                    return $"({input}).y";
                case 2:
                    return $"({input}).z";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}