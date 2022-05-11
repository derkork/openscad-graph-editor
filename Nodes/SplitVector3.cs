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
    /// Node for splitting a Vector3 into its components.
    /// </summary>
    [UsedImplicitly]
    public class SplitVector3 : ScadNode, IAmAnExpression, IHaveMultipleExpressionOutputs, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Split Vector3";
        public override string NodeDescription => "Splits a Vector3 into its components.";

        public SplitVector3()
        {
            InputPorts
                .Vector3(allowLiteral: false);
                
            OutputPorts
                .Number(allowLiteral: false)
                .Number(allowLiteral: false)
                .Number(allowLiteral: false);

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
                    return $"{input}.x";
                case 1:
                    return $"{input}.y";
                case 2:
                    return $"{input}.z";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.Vector3SplitIcon;
    }
}