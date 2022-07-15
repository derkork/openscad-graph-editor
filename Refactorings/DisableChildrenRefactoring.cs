using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Enables children for a <see cref="ModuleDescription"/>
    /// </summary>
    public class DisableChildrenRefactoring : Refactoring
    {
        private readonly ModuleDescription _moduleDescription;

        public DisableChildrenRefactoring(ModuleDescription moduleDescription)
        {
            _moduleDescription = moduleDescription;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (!_moduleDescription.SupportsChildren)
            {
                return; // nothing to do
            }
            
            // find all module invocations which refer to this module
            var affectedInvocations = context.Project.FindAllReferencingNodes(_moduleDescription)
                .Where(it => it.Node is ModuleInvocation)
                .ToList();

            _moduleDescription.SupportsChildren = false;
            
            // we need to remove the "children" input port from all affected invocations
            // all other input port connections need to move up one level
            foreach (var invocation in affectedInvocations)
            {
                var invocationNode = (ModuleInvocation) invocation.Node;
                var graph = invocation.Graph;

                // first up remove all connections that go to the "children" input port

                // delete all connections that go to the children output port
                var connectionsToChildren = graph.GetAllConnections()
                    .Where(it => it.InvolvesPort(invocationNode, PortId.Input(0)))
                    .ToList();
                
                // we can delete those immediately
                foreach (var connection in connectionsToChildren)
                {
                    graph.RemoveConnection(connection);
                }
                
                // now make a copy of all connections that go to the other input ports
                var otherInputConnections = graph.GetAllConnections()
                    .Where(it => it.To == invocationNode && it.ToPort > 0)
                    .ToList();

                // we can delete these now
                foreach (var connection in otherInputConnections)
                {
                    graph.RemoveConnection(connection);
                }
                
                // re-setup the ports.
                invocationNode.SetupPorts(_moduleDescription);
                
                // and re-add the other connections, but moved one port up
                foreach (var connection in otherInputConnections)
                {
                    graph.AddConnection(connection.From.Id, connection.FromPort, connection.To.Id, connection.ToPort - 1);
                }
            }
        }
    }
}