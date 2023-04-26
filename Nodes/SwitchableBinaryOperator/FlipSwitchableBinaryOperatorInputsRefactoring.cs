using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    [UsedImplicitly]
    public class FlipSwitchableBinaryOperatorInputsRefactoring : NodeRefactoring
    {
        public FlipSwitchableBinaryOperatorInputsRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (!(Node is SwitchableBinaryOperator switchableBinaryOperator))
            {
                return;
            }
            
            // first find the connection for the first input
            var firstInputConnection = Holder.GetAllConnections().FirstOrDefault(it => it.IsTo(Node, 0));
            // now the connection for the second input
            var secondInputConnection = Holder.GetAllConnections().FirstOrDefault(it => it.IsTo(Node, 1));
            
            // disconnect the connections
            if (firstInputConnection != null)
            {
                context.PerformRefactoring(new DeleteConnectionRefactoring(firstInputConnection));
            }

            if (secondInputConnection != null)
            {
                context.PerformRefactoring(new DeleteConnectionRefactoring(secondInputConnection));
            }

            // gather the port type from the input connections. use PortType.Any if there is no connection
            var firstPortType = firstInputConnection != null 
                ? firstInputConnection.From.GetPortType(PortId.Output(firstInputConnection.FromPort)) 
                : PortType.Any;
            
            var secondPortType = secondInputConnection != null 
                ? secondInputConnection.From.GetPortType(PortId.Output(secondInputConnection.FromPort)) 
                : PortType.Any;
            
            // now switch the port types
            context.PerformRefactoring(
                new SwitchBinaryOperatorInputPortTypesRefactoring(Holder, switchableBinaryOperator,
                    // note how this is deliberately flipped as we want to reverse the order of the inputs
                    secondPortType, firstPortType));
            
            // now reconnect the ports
            if (firstInputConnection != null)
            {
                var newConnection = new ScadConnection(firstInputConnection.Owner, firstInputConnection.From, firstInputConnection.FromPort, Node, 1);
                context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
            }
            
            if (secondInputConnection != null)
            {
                var newConnection = new ScadConnection(secondInputConnection.Owner, secondInputConnection.From, secondInputConnection.FromPort, Node, 0);
                context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
            }
            
        }
    }
}