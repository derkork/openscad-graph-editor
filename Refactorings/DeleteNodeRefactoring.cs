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
    /// the node will not be deleted and no connection will be deleted either. 
    /// </summary>
    internal class DeleteNodeRefactoring : NodeRefactoring
    {
        public DeleteNodeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (Node is ICannotBeDeleted)
            {
                return;
            }

            var connections = Holder.GetAllConnections()
                .Where(it => it.InvolvesNode(Node))
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
            Holder.RemoveNode(Node);
            
            // if the node implied children, check if the graph still has any node that implies children. If not unset
            // the "SupportsChildren" property.
            if (Node is IImplyChildren && Holder.Description is ModuleDescription moduleDescription)
            {
                var supportsChildren = Holder.GetAllNodes().Any(it => it is IImplyChildren);
                if (!supportsChildren)
                {
                    context.PerformRefactoring(new DisableChildrenRefactoring(moduleDescription));
                }
            }
        }
    }
}