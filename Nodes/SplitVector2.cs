using System;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Node for splitting a Vector2 into its components.
    /// </summary>
    [UsedImplicitly]
    public class SplitVector2 : ScadNode, IAmAnExpression, IHaveMultipleExpressionOutputs, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Split Vector2";
        public override string NodeDescription => "Splits a Vector2 into its components.";

        public SplitVector2()
        {
            InputPorts
                .Vector2(allowLiteral: false);

            OutputPorts
                .Number(allowLiteral:false)
                .Number(allowLiteral:false);

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.Vector2SplitIcon;
    }
}