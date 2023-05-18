using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Utils;
using Serilog;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Deletes a node. Will delete all connections from or to that node as well. Nodes that are marked as
    /// <see cref="ICannotBeDeleted"/> will not be deleted. If any connection to the node vetoes its disconnection
    /// the node will not be deleted and no connection will be deleted either. 
    /// </summary>
    internal class DeleteNodeRefactoring : NodeRefactoring
    {
        private readonly bool _retainConnectionsIfPossible;

        public DeleteNodeRefactoring(ScadGraph holder, ScadNode node, bool retainConnectionsIfPossible = false) : base(holder, node)
        {
            _retainConnectionsIfPossible = retainConnectionsIfPossible;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (Node is ICannotBeDeleted)
            {
                // if it cannot be deleted, nothing to do
                return;
            }

            if (!Holder.TryById(Node.Id, out _))
            {
                // already deleted, nothing to do
                return;
            }
            
            
            var connections = Holder.GetAllConnections()
                .Where(it => it.InvolvesNode(Node))
                .ToList();

            if (connections.Select(it => ConnectionRules.CanDisconnect(it))
                .Any(result => result.Decision == ConnectionRules.OperationRuleDecision.Veto))
            {
                Log.Information("Disconnect vetoed");
                return;
            }

            // drop all connections. Run a proper refactoring for this, so all the side effects are executed as well
            connections.Select(it => new DeleteConnectionRefactoring(it)).ForAll(context.PerformRefactoring);
            // and finally the node
            Holder.RemoveNode(Node);
            
            // if the deleted node was a reroute node, and we should retain connections, connect the source and target(s)
            // of the reroute node
            
            if (_retainConnectionsIfPossible && Node is RerouteNode)
            {
               var sourceConnection = connections
                   .FirstOrDefault(it => it.To == Node);
               
                var targetsConnections = connections
                    .Where(it => it.From == Node)
                    .ToList();
                
                if (sourceConnection != null && targetsConnections.Any())
                {
                    // Create new direct connections between the source and the targets
                    foreach (var targetsConnection in targetsConnections)
                    {
                        var newConnection = 
                            new ScadConnection(Holder, sourceConnection.From,sourceConnection.FromPort, targetsConnection.To, targetsConnection.ToPort);
                        context.PerformRefactoring(new AddConnectionRefactoring(newConnection));
                    }
                    
                }
            }
            
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
            
            // if the node is bound to another node and the other node still exists, delete the other node as well.
            if (Node is IAmBoundToOtherNode boundToOtherNode &&
                Holder.TryById(boundToOtherNode.OtherNodeId, out var otherNode))
            {
                context.PerformRefactoring(new DeleteNodeRefactoring(Holder, otherNode));
            }
        }
    }
}