using System.Text;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ForComprehension
{
    /// <summary>
    /// For comprehension node for building lists from other lists.
    /// </summary>
    [UsedImplicitly]
    public class ForComprehension : ScadNode, IAmAnExpression, IHaveMultipleExpressionOutputs, ICanConnectToMyself
    {
        public override string NodeTitle => "Map ('for'-comprehension)";
        public override string NodeDescription => "Maps a list or range into a new list. Also known as a 'for' list comprehension.";

        public int NestLevel { get; private set; } = 1;

        static ForComprehension()
        {
            // When connecting to myself veto all connections that do not go to the result port.
            ConnectionRules.AddConnectRule(it => it.From == it.To
                                                 && (it.From is ForComprehension forComprehension)
                                                 && it.ToPort != forComprehension.NestLevel,
                ConnectionRules.OperationRuleDecision.Veto);
        }

        public ForComprehension()
        {
            RebuildPorts();
        }

        private void RebuildPorts()
        {
            InputPorts
                .Clear();

            OutputPorts
                .Clear();

            OutputPorts
                .Array("Result");

            for (var i = 0; i < NestLevel; i++)
            {
                InputPorts.Array($"Vector {i + 1}");
                OutputPorts.Any($"Vector Element {i + 1}");
            }

            InputPorts
                .Any("Result");
        }

        /// <summary>
        /// Adds a nested for loop level. The caller is responsible for fixing up port connections.
        /// </summary>
        public void IncreaseNestLevel()
        {
            NestLevel += 1;
            RebuildPorts();
            // since we have no literals here, we can skip re-building port literals
        }

        /// <summary>
        /// Removes a nested for loop level. The caller is responsible for fixing up port connections.
        /// </summary>
        public void DecreaseNestLevel()
        {
            GdAssert.That(NestLevel > 1, "Cannot decrease nest level any further.");
            NestLevel -= 1;
            RebuildPorts();
            // since we have no literals here, we can skip re-building port literals
        }


        public override void SaveInto(SavedNode node)
        {
            node.SetData("nest_level", NestLevel);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            NestLevel = node.GetDataInt("nest_level", 1);
            RebuildPorts();
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public override string Render(IScadGraph context)
        {
            return "";
        }
        

        public string RenderExpressionOutput(IScadGraph context, int port)
        {

            if (port == 0) // result port
            {
                var innerExpression = RenderInput(context, NestLevel);
                if (innerExpression.Empty())
                {
                    return "";
                }

                var builder = new StringBuilder("[for(");
                for (var i = 0; i < NestLevel; i++)
                {
                    var loopVarName = Id.UniqueStableVariableName(i);
                    var array = RenderInput(context, i).OrDefault("[]");
                    builder.Append(loopVarName)
                        .Append(" = ")
                        .Append(array);
                    if (i + 1 < NestLevel)
                    {
                        builder.Append(", ");
                    }
                }
                builder.Append(") ")
                    .Append("(")
                    .Append(innerExpression)
                    .Append(")")
                    .Append("]");
                return builder.ToString();
            }

            GdAssert.That(port > 0 && port <= NestLevel, "port out of range");
            return Id.UniqueStableVariableName(port - 1);
        }
    }
}