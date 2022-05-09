using System.Linq;
using GodotExt;
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
            
            // we need to add a new "children" output port to the invocation
            foreach (var invocation in affectedInvocations)
            {
                var invocationNode = (ModuleInvocation) invocation.Node;
                var graph = invocation.Graph;

                // the node should have exactly 1 output port right now
                GdAssert.That(invocationNode.OutputPortCount == 1,
                    "Module invocation should have exactly 1 output port");

                // the connection from this output port needs to move one level down because we're adding a new output port
                // so save these connections.
                var connections = graph.GetAllConnections()
                    .Where(it => it.InvolvesPort(invocationNode, PortId.Output(0)))
                    .ToList();

                // re-setup the ports.
                invocationNode.SetupPorts(_moduleDescription);


                // kill all the connections
                foreach (var connection in connections)
                {
                    graph.RemoveConnection(connection);
                }

                // now re-insert all the connections using a modified port
                foreach (var connection in connections)
                {
                    graph.AddConnection(connection.From.Id, connection.FromPort + 1, connection.To.Id,
                        connection.ToPort);
                }
            }
        }
    }
}