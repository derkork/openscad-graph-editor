using System.Linq;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Deletes a node. Will delete all connections from or to that node as well. Nodes that are marked as
    /// <see cref="ICannotBeDeleted"/> will not be deleted. If any connection to the node vetoes its disconnection
    /// the node will not be deleted and no connection will be deleted either. This is internally  used. External
    /// classes should use <see cref="DeleteNodeRefactoring"/> which will ensure proper node destruction for all
    /// kinds of nodes.
    /// </summary>
    internal class DeleteNodeRefactoringSimple : NodeRefactoring
    {
        public DeleteNodeRefactoringSimple(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (Node is ICannotBeDeleted)
            {
                return;
            }

            var graph = context.MakeRefactorable(Holder);
            var toDelete = graph.ById(Node.Id);

            var connections = graph.GetAllConnections()
                .Where(it => it.InvolvesNode(toDelete))
                .ToList();

            if (connections.Select(ConnectionRules.CanDisconnect)
                .Any(result => result.Decision == ConnectionRules.OperationRuleDecision.Veto))
            {
                GD.Print("Disconnection vetoed.");
                return;
            }

            // drop all connections. Run a proper refactoring for this, so all the side effects are executed as well
            connections.Select(it => new DeleteConnectionRefactoring(it)).ForAll(context.PerformRefactoring);
            // and finally the node
            graph.RemoveNode(toDelete);
        }
    }
}