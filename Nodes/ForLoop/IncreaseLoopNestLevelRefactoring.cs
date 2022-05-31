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

        public IncreaseLoopNestLevelRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (ForLoop) Node;
            // when increasing nest level, we need to fix the connection to the "After" port, as it moves one port down.
            var afterPort = node.NestLevel + 1;

            node.IncreaseNestLevel();
            
            var existingConnection = Holder.GetAllConnections().FirstOrDefault(it => it.IsFrom(node, afterPort));
            if (existingConnection != null)
            {
                Holder.RemoveConnection(existingConnection);
                Holder.AddConnection(existingConnection.From.Id, existingConnection.FromPort +1,
                    existingConnection.To.Id, existingConnection.ToPort);
            }
        }
    }
}