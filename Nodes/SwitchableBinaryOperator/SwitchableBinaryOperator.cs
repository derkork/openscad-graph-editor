using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    public abstract class SwitchableBinaryOperator : BinaryOperator
    {

        protected SwitchableBinaryOperator()
        {
            InputPorts
                // we start with `Any` for both input port as usually you will connect another node there
                // and therefore this is the most suitable setting.
                .Any() 
                .Any();

            OutputPorts
                .PortType(PortType.Any);
        }

        static SwitchableBinaryOperator()
        {
            // connecting to switchable binary operator input will automatically switch the input port 
            // output ports and fix any adjacent connections
            ConnectionRules.AddConnectRule(
                it => it.To is SwitchableBinaryOperator,
                ConnectionRules.OperationRuleDecision.Undecided,
                it => new FixSwitchableBinaryOperatorPortTypesRefactoring(it.Owner, it.To, it.ToPort)
            );

            // same but for disconnection from a binary operator input
            ConnectionRules.AddDisconnectRule(
                it => it.To is SwitchableBinaryOperator,
                ConnectionRules.OperationRuleDecision.Undecided,
                it => new FixSwitchableBinaryOperatorPortTypesRefactoring(it.Owner, it.To, it.ToPort)
            );
            
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("first_operand_port_type", (int) InputPorts[0].PortType);
            node.SetData("second_operand_port_type", (int) InputPorts[1].PortType);
            base.SaveInto(node);
        }

        public abstract bool Supports(PortType portType);

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver resolver)
        {
            var firstOperandPortType = (PortType) node.GetDataInt("first_operand_port_type", (int) PortType.Any);
            var secondOperandPortType = (PortType) node.GetDataInt("second_operand_port_type", (int) PortType.Any);
            InputPorts.Clear();
            InputPorts
                .PortType(firstOperandPortType, literalType: firstOperandPortType.GetMatchingLiteralType())
                .PortType(secondOperandPortType, literalType: secondOperandPortType.GetMatchingLiteralType());

            base.RestorePortDefinitions(node, resolver);
        }

        public void SwitchPortType(PortId port, PortType newPortType)
        {
            GdAssert.That(port.IsInput && port.Port < InputPorts.Count, "Invalid port id");
            if (GetPortType(port) == newPortType)
            {
                return; // nothing to do.
            }
            
            var existingDefinition = InputPorts[port.Port];
            InputPorts[port.Port] = new PortDefinition(newPortType, newPortType.GetMatchingLiteralType(), existingDefinition.Name,
                existingDefinition.LiteralIsAutoSet, existingDefinition.DefaultValue, existingDefinition.RenderHint);

            var wasSet = false;
            if (TryGetLiteral(port, out var literal))
            {
                wasSet = literal.IsSet;
            }
            
            DropPortLiteral(port);
            BuildPortLiteral(port);

            if (TryGetLiteral(port, out var newLiteral))
            {
                newLiteral.IsSet = wasSet;
            }
        }
    }
}