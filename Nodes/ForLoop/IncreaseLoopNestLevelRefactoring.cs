using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.ForLoop
{
    [UsedImplicitly]
    public class IncreaseLoopNestLevelRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Increase Loop Nest Level";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is ForLoop;

        public IncreaseLoopNestLevelRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (ForLoop) graph.ById(Node.Id);
            // when increasing nest level, we need to fix the connection to the "After" port, as it moves one port down.
            var afterPort = node.NestLevel + 1;

            node.IncreaseNestLevel();
            
            var existingConnection = graph.GetAllConnections().FirstOrDefault(it => it.IsFrom(node, afterPort));
            if (existingConnection != null)
            {
                graph.RemoveConnection(existingConnection);
                graph.AddConnection(existingConnection.From.Id, existingConnection.FromPort +1,
                    existingConnection.To.Id, existingConnection.ToPort);
            }
        }
    }
}