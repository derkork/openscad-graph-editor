using System;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class ForLoop : ScadNode, IMultiExpressionOutputNode
    {
        public override string NodeTitle => "For Each";
        public override string NodeDescription => "Executes its children for each entry in the given array.";

        public ForLoop()
        {
            InputPorts
                .Flow()
                .Array("Array");

            OutputPorts
                .Flow("Children")
                .Any("Array Element")
                .Flow("After");
        }

        public override string Render(IScadGraph context)
        {
            var loopVarName = Id.UniqueStableVariableName(0);

            var array = RenderInput(context, 1);
            var children = RenderOutput(context, 0);
            var next = RenderOutput(context, 2);

            return $"for({loopVarName} = {array}){children.AsBlock()}\n{next}";
        }

        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            switch (port)
            {
                case 1:
                    return Id.UniqueStableVariableName(0);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsExpressionPort(int port)
        {
            return port == 1;
        }
    }
}