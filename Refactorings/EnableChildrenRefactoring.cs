using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Enables children for a <see cref="ModuleDescription"/>
    /// </summary>
    public class EnableChildrenRefactoring : Refactoring
    {
        private readonly ModuleDescription _moduleDescription;

        public EnableChildrenRefactoring(ModuleDescription moduleDescription)
        {
            _moduleDescription = moduleDescription;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (_moduleDescription.SupportsChildren)
            {
                return; // nothing to do
            }
            
            // find all module invocations which refer to this module
            var affectedInvocations = context.Project.FindAllReferencingNodes(_moduleDescription)
                .Where(it => it.Node is ModuleInvocation)
                .ToList();

            _moduleDescription.SupportsChildren = true;
            
            // We need to add an input port to each invocation which refers to this module.
            // The children input port is added to the top spot, so we need to move all
            // connections to other modules down one spot.
            foreach (var invocation in affectedInvocations)
            {
                var invocationNode = (ModuleInvocation) invocation.Node;
                var graph = invocation.Graph;

                // first find all input connections to this invocation
                var savedConnections = graph.GetAllConnections()
                    .Where(it => it.To == invocationNode)
                    .ToList();
                
                // we need to re-move all these connections
                foreach (var connection in savedConnections)
                {
                    graph.RemoveConnection(connection);
                }
                
                // re-setup the ports.
                invocationNode.SetupPorts(_moduleDescription);
                
                // now re-add all the connections but moved one port down
                foreach (var connection in savedConnections)
                {
                    graph.AddConnection(connection.From.Id, connection.FromPort, connection.To.Id, connection.ToPort + 1);
                }
            }
        }
    }
}