using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes.Reroute
{
    [UsedImplicitly]
    public class RerouteNode : ScadNode, IHaveCustomWidget, IHaveNodeBackground
    {
        public override string NodeTitle => "Reroute";
        public override string NodeDescription => "Rerouting node which aids in making cleaner visual graphs.";

        public bool IsWireless { get; set; }

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
                .OfType(PortType.Reroute);
            OutputPorts
                .OfType(PortType.Reroute);
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (GetPortType(portId) == PortType.Geometry)
            {
                if (portId.IsInput)
                {
                    return "Input geometry";
                }

                if (portId.IsOutput)
                {
                    return "Output geometry";
                }
            }
            else
            {
                if (portId.IsInput)
                {
                    return "Input value";
                }

                if (portId.IsOutput)
                {
                    return "Output value";
                }
            }

            return "";
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("reroute_type", (int) GetPortType(PortId.Input(0)));
            node.SetData("is_wireless", IsWireless);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver referenceResolver)
        {
            IsWireless = node.GetDataBool("is_wireless");
            var type = (PortType) node.GetDataInt("reroute_type");
            UpdatePortType(type);
            base.RestorePortDefinitions(node, referenceResolver);
        }

        public void UpdatePortType(PortType type)
        {
            InputPorts.Clear();
            OutputPorts.Clear();
            InputPorts
                .OfType(type);
            OutputPorts
                .OfType(type);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            return RenderInput(context, portIndex);
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<RerouteNodeWidget>();
        }

        public Texture NodeBackground => IsWireless ? Resources.WirelessIcon : null;
    }
}