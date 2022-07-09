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
                // we start with `Any` for the first input port as usually you will connect another node there
                // and therefore this is the most suitable setting.
                .Any() 
                // number is going to be the most common case for the second operator, so we start with this by default
                .Number();

            OutputPorts
                .OfType(PortType.Any);
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
                .OfType(firstOperandPortType, literalType: firstOperandPortType.GetMatchingLiteralType())
                .OfType(secondOperandPortType, literalType: secondOperandPortType.GetMatchingLiteralType());

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
            InputPorts[port.Port] = PortDefinition.OfType(
                newPortType, 
                existingDefinition.Name, 
                newPortType.GetMatchingLiteralType(),
                existingDefinition.LiteralIsAutoSet);

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