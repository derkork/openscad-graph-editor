using System.Linq;
using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes.Sum
{
    /// <summary>
    /// The sum node is a special node that sums all its inputs.
    /// </summary>
    [UsedImplicitly]
    public class Sum : ScadNode, ICanHaveMultipleInputConnections, IHaveCustomWidget, IHaveNodeBackground, IAmAnExpression
    {
        public override string NodeTitle => "Sum";

        public override string NodeDescription =>
            "Sums all inputs. Supports numbers and vector types as long as they are not mixed.";

        public override string NodeQuickLookup => "Smu";


        public Sum()
        {
            // initialize input and output ports with Any as this is the most permissive type
            InputPorts.Any();
            OutputPorts.Any();
        }

        static Sum()
        {
            // Connecting anything to the SUM node will make it re-evaluate the inputs and outputs
            // connecting to switchable binary operator input will automatically switch the input port 
            // output ports and fix any adjacent connections
            ConnectionRules.AddConnectRule(
                it => it.To is Sum,
                ConnectionRules.OperationRuleDecision.Undecided,
                it => new FixSumPortTypesRefactoring(it.Owner, (Sum) it.To)
            );

            // disconnecting anything from the SUM node will make it re-evaluate the inputs and outputs
            ConnectionRules.AddDisconnectRule(
                it => it.To is Sum,
                ConnectionRules.OperationRuleDecision.Undecided,
                it => new FixSumPortTypesRefactoring(it.Owner, (Sum) it.To)
            );

            // only allow connections to the sum node if the originating port type is one of:
            // - Any
            // - Number
            // - Vector
            // - Vector2
            // - Vector3
            ConnectionRules.AddConnectRule(
                it => it.To is Sum
                      && it.TryGetFromPortType(out var type)
                      && !(type == PortType.Any || type == PortType.Number || type == PortType.Vector ||
                           type == PortType.Vector2 || type == PortType.Vector3),
                ConnectionRules.OperationRuleDecision.Veto
            );
            
            // you can connect a non-matching type to the sum node but this will remove all connections
            // which are not of a compatible type
            // connecting to a switchable binary operator input is possible
            // if the port type is supported by the operator, even if it is currently
            // not the correct type
            ConnectionRules.AddConnectRule(
                it => it.To is Sum sum && it.TryGetFromPortType(out var type) && !type.CanBeAssignedTo(
                    sum.GetPortType(PortId.Input(it.ToPort))),
                ConnectionRules.OperationRuleDecision.Allow,
                it =>
                {
                    it.TryGetFromPortType(out var type); // should work because of the rule above
                    return new DeleteUnassignableConnectionsRefactoring(it.Owner, (Sum) it.To, type);
                });
        }

        public override string GetPortDocumentation(PortId portId)
        {
            if (portId.IsInput)
            {
                return
                    "Input values. You can connect any number of nodes to this port and their values will be summed.";
            }

            return "The sum of all inputs.";
        }


        /// <summary>
        /// Switches the port type of the given port to the new type.
        /// </summary>
        /// <param name="newPortType"></param>
        public void SwitchPortType(PortType newPortType)
        {
            if (GetPortType(PortId.Input(0)) == newPortType)
            {
                return; // nothing to do.
            }

            var existingDefinition = GetPortDefinition(PortId.Input(0));

            var newDefinition = new PortDefinition(newPortType, LiteralType.None, existingDefinition.Name,
                existingDefinition.LiteralIsAutoSet, existingDefinition.DefaultValue, existingDefinition.RenderHint);

            SetPortDefinition(PortId.Input(0), newDefinition);

            // repeat for the output port
            existingDefinition = GetPortDefinition(PortId.Output(0));

            var newOutputDefinition = new PortDefinition(newPortType, LiteralType.None, existingDefinition.Name,
                existingDefinition.LiteralIsAutoSet, existingDefinition.DefaultValue, existingDefinition.RenderHint);

            SetPortDefinition(PortId.Output(0), newOutputDefinition);
        }

        public override string Render(ScadGraph context, int portIndex)
        {
            if (portIndex != 0)
            {
                return "";
            }

            // this one's a bit special so we need to do our own rendering. 
            // first get all input nodes
            var result = context.GetAllConnections()
                .Where(it => it.IsTo(this, 0))
                // render their outputs
                .Select(it => it.From.Render(context, it.FromPort))
                // filter out empty strings
                .Where(it => !string.IsNullOrEmpty(it))
                // and join them together
                .JoinToString(" + ");

            // if we have at least one "+" in the expression, we need to wrap it in parentheses
            if (result.Contains("+"))
            {
                result = $"({result})";
            }

            return result;
        }

        public override void SaveInto(SavedNode node)
        {
            // input and output port type always match
            node.SetData("port_type", (int) InputPorts[0].PortType);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver resolver)
        {
            var portType = (PortType) node.GetDataInt("port_type", (int) PortType.Any);
            InputPorts.Clear();
            InputPorts
                .PortType(portType, literalType: LiteralType.None);
            OutputPorts.Clear();
            OutputPorts
                .PortType(portType, literalType: LiteralType.None);

            base.RestorePortDefinitions(node, resolver);
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<SmallNodeWidget>();
        }

        public Texture NodeBackground => Resources.SumIcon;
    }
}