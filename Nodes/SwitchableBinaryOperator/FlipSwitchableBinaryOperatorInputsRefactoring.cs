using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    [UsedImplicitly]
    public class FlipSwitchableBinaryOperatorInputsRefactoring : UserSelectableNodeRefactoring
    {

        public FlipSwitchableBinaryOperatorInputsRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override int Order => 10;

        public override string Title => "Flip inputs";


        public override bool IsApplicableToNode => Node is SwitchableBinaryOperator;

        public override void PerformRefactoring(RefactoringContext context)
        {
            
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

            // if the first connection exists, find out its originating port type
            if (firstInputConnection != null)
            {
                var originatorPortType =
                    firstInputConnection.From.GetPortType(PortId.Output(firstInputConnection.FromPort));
                
                // now switch the node's second port to the type of the first port
                context.PerformRefactoring(new SwitchBinaryOperatorPortTypeRefactoring(Holder, Node, false, originatorPortType));
            }
            
            // report the same for the second port
            if (secondInputConnection != null)
            {
                var originatorPortType =
                    secondInputConnection.From.GetPortType(PortId.Output(secondInputConnection.FromPort));
                
                context.PerformRefactoring(new SwitchBinaryOperatorPortTypeRefactoring(Holder, Node, true, originatorPortType));
            }
            
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