using System;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes.Reroute
{
    [UsedImplicitly]
    public class RerouteNode : ScadNode, IMultiExpressionOutputNode
    {
        public override string NodeTitle => "Reroute";
        public override string NodeDescription => "Rerouting node which aids in making cleaner visual graphs.";

        static RerouteNode()
        {
            // connecting FROM a reroute node, will implicitly switch the reroute node's type
            // to the type of the target port
            ConnectionRules.AddConnectRule(
                    it => it.IsFromPortType(PortType.Reroute),
                    ConnectionRules.OperationRuleDecision.Allow,
                    it => new FixRerouteTypeRefactoring(it.Owner, it.From)
                );


            // connecting TO a reroute node, will implicitly switch the reroute node's type
            // to the type of the originating port
            ConnectionRules.AddConnectRule(
                it => it.IsToPortType(PortType.Reroute),
                ConnectionRules.OperationRuleDecision.Allow,
                it => new FixRerouteTypeRefactoring(it.Owner, it.To)
            );


            // when you disconnect from a reroute node output, switch its type back to "Reroute"
            // if nothing more is connected to it.
            ConnectionRules.AddDisconnectRule(
                it => it.From is RerouteNode,
                ConnectionRules.OperationRuleDecision.Undecided,
                it => new FixRerouteTypeRefactoring(it.Owner, it.From)
            );

            // same but for disconnection from a reroute input
            ConnectionRules.AddDisconnectRule(
                it => it.To is RerouteNode,
                ConnectionRules.OperationRuleDecision.Undecided,
                it => new FixRerouteTypeRefactoring(it.Owner, it.To)
            );
        }


        public RerouteNode()
        {
            InputPorts
                .OfType(PortType.Reroute, allowLiteral: false);
            OutputPorts
                .OfType(PortType.Reroute, allowLiteral: false);
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("reroute_type", (int) GetInputPortType(0));
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            var type = (PortType) node.GetDataInt("reroute_type");
            UpdatePortType(type);
        }

        public void UpdatePortType(PortType type)
        {
            InputPorts.Clear();
            OutputPorts.Clear();
            InputPorts
                .OfType(type, allowLiteral: false);
            OutputPorts
                .OfType(type, allowLiteral: false);
        }

        public override string Render(IScadGraph context)
        {
            if (GetOutputPortType(0) != PortType.Flow)
            {
                throw new InvalidOperationException("Cannot render non-flow type");
            }

            return RenderOutput(context, 0);
        }


        public string RenderExpressionOutput(IScadGraph context, int port)
        {
            var outputPortType = GetOutputPortType(0);
            if (outputPortType == PortType.Flow || outputPortType == PortType.Reroute)
            {
                throw new InvalidOperationException("Cannot render non-expression type.");
            }

            return RenderInput(context, 0);
        }

        public bool IsExpressionPort(int port)
        {
            if (port != 0)
            {
                return false;
            }

            var outputPortType = GetOutputPortType(0);
            return outputPortType != PortType.Flow && outputPortType != PortType.Reroute;
        }
    }
}