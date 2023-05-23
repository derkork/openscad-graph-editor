using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    public abstract class SwitchableBinaryOperator : BinaryOperator
    {

        protected SwitchableBinaryOperator()
        {
            InputPorts
                // we start with `Any` as it is the most compatible
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
                it => it.To is SwitchableBinaryOperator ,
                ConnectionRules.OperationRuleDecision.Undecided,
                it => new FixSwitchableBinaryOperatorPortTypesRefactoring(it.Owner, (SwitchableBinaryOperator) it.To)
            );

            // same but for disconnection from a binary operator input
            ConnectionRules.AddDisconnectRule(
                it => it.To is SwitchableBinaryOperator,
                ConnectionRules.OperationRuleDecision.Undecided,
                it => new FixSwitchableBinaryOperatorPortTypesRefactoring(it.Owner, (SwitchableBinaryOperator) it.To)
            );
            
            // connecting to a switchable binary operator input is possible
            // if the port type is supported by the operator, even if it is currently
            // not the correct type
            ConnectionRules.AddConnectRule(
                it => it.To is SwitchableBinaryOperator bo && it.TryGetFromPortType(out var type) && bo.Supports(type) ,
                ConnectionRules.OperationRuleDecision.Allow);
            
            // if the operator does not support the port type, it's a hard veto
            ConnectionRules.AddConnectRule(
                it => it.To is SwitchableBinaryOperator bo && it.TryGetFromPortType(out var type) && !bo.Supports(type) ,
                ConnectionRules.OperationRuleDecision.Veto);
        }

        public override void SaveInto(SavedNode node)
        {
            node.SetData("first_operand_port_type", (int) InputPorts[0].PortType);
            node.SetData("second_operand_port_type", (int) InputPorts[1].PortType);
            base.SaveInto(node);
        }

        public abstract bool Supports(PortType portType);
        
        
        /// <summary>
        /// Checks if the given input port types are supported by this operator in this combination and returns
        /// the result port type. If the combination is not supported, the result port type is set to `PortType.Any`.
        /// </summary>
        public abstract bool Supports(PortType first, PortType second, out PortType resultPortType);

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver resolver)
        {
            var firstOperandPortType = (PortType) node.GetDataInt("first_operand_port_type", (int) PortType.Any);
            var secondOperandPortType = (PortType) node.GetDataInt("second_operand_port_type", (int) PortType.Any);
            SwitchInputPorts(firstOperandPortType, secondOperandPortType);

            base.RestorePortDefinitions(node, resolver);
        }

        
        public void SwitchInputPorts(PortType firstPortType, PortType secondPortType)
        {
            SwitchPortType(PortId.Input(0), firstPortType);
            SwitchPortType(PortId.Input(1), secondPortType);

            var outputPortType = CalculateOutputPortType();
            SwitchPortType(PortId.Output(0), outputPortType);
        }
        
        private void SwitchPortType(PortId port, PortType newPortType)
        {
            
            if (GetPortType(port) == newPortType)
            {
                return; // nothing to do.
            }

            var existingDefinition = GetPortDefinition(port);
            
            var newDefinition = new PortDefinition(newPortType, 
                // the output port will never have a literal
                port.IsOutput ? LiteralType.None : newPortType.GetMatchingLiteralType(),
                existingDefinition.Name,
                existingDefinition.LiteralIsAutoSet, existingDefinition.DefaultValue, existingDefinition.RenderHint);
            
            SetPortDefinition(port, newDefinition);

            // for input ports we may need to rebuild the literal

            if (!port.IsInput)
            {
                // the output port of this node will never have a literal, so we are done here.
                return;
            }

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

        /// <summary>
        /// Calculates the output port type based on the currently set input port types. Should return
        /// <see cref="PortType.Any"/> if the output port type cannot be determined or the input port type
        /// combination is not supported.
        /// </summary>
        /// <returns></returns>
        protected abstract PortType CalculateOutputPortType();

    }
}