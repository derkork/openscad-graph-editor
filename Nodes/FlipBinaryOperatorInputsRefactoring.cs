﻿using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes.SwitchableBinaryOperator;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class FlipBinaryOperatorInputsRefactoring : UserSelectableNodeRefactoring
    {

        public FlipBinaryOperatorInputsRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override int Order => 10;

        public override string Title => "Flip inputs";


        public override bool IsApplicableToNode => Node is BinaryOperator && !(Node is SwitchableBinaryOperator.SwitchableBinaryOperator);

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
            
            // binary operators have no switchable types so we can just re-attach the connections flipped
            
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