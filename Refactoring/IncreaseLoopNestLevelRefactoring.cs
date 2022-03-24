using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactoring
{
    public class IncreaseLoopNestLevelRefactoring : NodeRefactoring
    {
        public override string Title => "Increase Loop Nest Level";

        public override bool Applies => Node is ForLoop;

        public IncreaseLoopNestLevelRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(ScadProject project)
        {
            var graph = MakeRefactorable(Holder, project);
            var node = (ForLoop) graph.ById(Node.Id);
            // when increasing nest level, we need to fix the connection to the "After" port, as it moves one port down.
            var afterPort = node.NestLevel + 1;

            node.IncreaseNestLevel();
            
            var existingConnection = graph.GetAllConnections().FirstOrDefault(it => it.IsFrom(node, afterPort));
            if (existingConnection != null)
            {
                graph.Remove(existingConnection);
                var newConnection = new ScadConnection(existingConnection.From, existingConnection.FromPort + 1,
                    existingConnection.To, existingConnection.ToPort);
                graph.Add(newConnection);
            }
            
            TransferModifiedDataBackToVisibleGraphs(project);            
        }
    }
}