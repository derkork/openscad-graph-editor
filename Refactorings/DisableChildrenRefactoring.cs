using System.Linq;
using GodotExt;
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
            
            // we need to remove the "children" output port from the invocation
            foreach (var invocation in affectedInvocations)
            {
                var invocationNode = (ModuleInvocation) invocation.Node;
                var graph = invocation.Graph;

                // the node should have exactly 2 output ports right now
                GdAssert.That(invocationNode.OutputPortCount == 2,
                    "Module invocation should have exactly 2 output ports");

                // the connection from the second output port needs to move up one level because we're going to remove the
                // children output port. so save these connections.
                var connections = graph.GetAllConnections()
                    .Where(it => it.InvolvesPort(invocationNode, PortId.Output(1)))
                    .ToList();
                
                // also collect all connections that go to the children output port
                var connectionsToChildren = graph.GetAllConnections()
                    .Where(it => it.InvolvesPort(invocationNode, PortId.Output(0)))
                    .ToList();
                
                // we can delete those immediately
                foreach (var connection in connectionsToChildren)
                {
                    graph.RemoveConnection(connection);
                }

                // re-setup the ports.
                invocationNode.SetupPorts(_moduleDescription);


                // kill all the connections that were saved
                foreach (var connection in connections)
                {
                    graph.RemoveConnection(connection);
                }

                // now re-insert all the connections using a modified port
                foreach (var connection in connections)
                {
                    graph.AddConnection(connection.From.Id, connection.FromPort -1, connection.To.Id,
                        connection.ToPort);
                }
            }
        }
    }
}