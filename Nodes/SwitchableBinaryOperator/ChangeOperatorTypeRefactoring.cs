using System;
using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    public class ChangeOperatorTypeRefactoring : NodeRefactoring
    {
        private readonly Type _nodeType;

        public ChangeOperatorTypeRefactoring(ScadGraph holder, SwitchableBinaryOperator node, Type nodeType) : base(holder, node)
        {
            _nodeType = nodeType;
        }


        public override void PerformRefactoring(RefactoringContext context)
        {
            var oldNode = (SwitchableBinaryOperator) Node;
            var newNode = (SwitchableBinaryOperator) NodeFactory.Build(_nodeType);
            
            // first add the new node to the graph
            context.PerformRefactoring(new AddNodeRefactoring(Holder, newNode));
            // we put the new node in the same position as the old one
            context.PerformRefactoring(new ChangeNodePositionRefactoring(Holder, newNode, oldNode.Offset));
            
            // we switch the port types to the same port types as the old node (if supported, or 'any' if not)
            // the `IsApplicableToNode` check ensures that the new operator type supports both operand types of the old one
            
            var firstInputTargetPortType = oldNode.GetPortType(PortId.Input(0));
            if (!newNode.Supports(firstInputTargetPortType))
            {
                firstInputTargetPortType = PortType.Any;
            }

            // repeat for second input port
            var secondInputTargetPortType = oldNode.GetPortType(PortId.Input(1));
            if (!newNode.Supports(secondInputTargetPortType))
            {
                secondInputTargetPortType = PortType.Any;
            }
            
            context.PerformRefactoring(new SwitchBinaryOperatorInputPortTypesRefactoring(Holder, newNode, firstInputTargetPortType, secondInputTargetPortType));
                
            // now copy over any literal values from the old node to the new one
            if (oldNode.TryGetLiteral(PortId.Input(0), out var firstInputLiteral))
            {
                var has = newNode.TryGetLiteral(PortId.Input(0), out var newFirstInputLiteral);
                GdAssert.That(has, "new node should have literal value for first input");
                newFirstInputLiteral.SerializedValue = firstInputLiteral.SerializedValue;
                newFirstInputLiteral.IsSet = firstInputLiteral.IsSet;
            }
            
            // repeat for second input port
            if (oldNode.TryGetLiteral(PortId.Input(1), out var secondInputLiteral))
            {
                var has = newNode.TryGetLiteral(PortId.Input(1), out var newSecondInputLiteral);
                GdAssert.That(has, "new node should have literal value for second input");
                newSecondInputLiteral.SerializedValue = secondInputLiteral.SerializedValue;
                newSecondInputLiteral.IsSet = secondInputLiteral.IsSet;
            }
                
            // now copy all the connections that were made to the old node to the new one
            var connectionsToFirstInputPort = Holder.GetAllConnections()
                .Where(it => it.IsTo(oldNode, 0))
                .ToList();
            var connectionsToSecondInputPort = Holder.GetAllConnections()
                .Where(it => it.IsTo(oldNode, 1))
                .ToList();
            var connectionsFromOutputPort = Holder.GetAllConnections()
                .Where(it => it.IsFrom(oldNode, 0))
                .ToList();

            foreach (var connection in connectionsToFirstInputPort)
            {
                context.PerformRefactoring(new AddConnectionRefactoring(new ScadConnection(Holder, connection.From, connection.FromPort, newNode, connection.ToPort)));
            }

            foreach (var connection in connectionsToSecondInputPort)
            {
                context.PerformRefactoring(new AddConnectionRefactoring(new ScadConnection(Holder, connection.From, connection.FromPort, newNode, connection.ToPort)));
            }

            foreach (var connection in connectionsFromOutputPort)
            {
                context.PerformRefactoring(new AddConnectionRefactoring(new ScadConnection(Holder, newNode, connection.FromPort, connection.To, connection.ToPort)));
            }
            
            // finally, remove the old node 
            context.PerformRefactoring(new DeleteNodeRefactoring(Holder, oldNode));
        }
    }
}