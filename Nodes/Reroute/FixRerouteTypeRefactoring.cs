using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Reroute
{
    /// <summary>
    /// Fixes reroute type to match the connected nodes.
    /// </summary>
    public class FixRerouteTypeRefactoring : NodeRefactoring
    {
        public FixRerouteTypeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override bool IsLate => true;

        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (RerouteNode) graph.ById(Node.Id);

            var connections = graph
                .GetAllConnections()
                .Where(it => it.InvolvesNode(node))
                .ToList();
                
            // if input is connected, and originating type is not "Reroute" then use this type
            foreach (var connection in connections.Where(connection => connection.IsTo(node, 0)))
            {
                if (connection.TryGetFromPortType(out var type) && type != PortType.Reroute) 
                {
                    // update the port type
                    node.UpdatePortType(type);
                    return;
                }
            }
            
            // no good connections on input so lets check the outputs
            foreach (var connection in connections.Where(connection => connection.IsFrom(node, 0)))
            {
                if (connection.TryGetToPortType(out var type) && type != PortType.Reroute) 
                {
                    // update the port type
                    node.UpdatePortType(type);
                    return;
                }
            }
            
            // no connections on either side, so lets use the default
            node.UpdatePortType(PortType.Reroute);
        }
    }
}