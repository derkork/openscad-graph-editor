using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.ForComprehension
{
    [UsedImplicitly]
    public class IncreaseForComprehensionDimension : UserSelectableNodeRefactoring
    {
        public override string Title => "Increase dimension";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is ForComprehension;

        public IncreaseForComprehensionDimension(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (ForComprehension) Node;
            // when increasing nest level, we need to fix the connection to the "Result" input port, as it moves one port down.
            var resultInputPort = node.NestLevel;

            node.IncreaseNestLevel();
            
            var existingConnection = Holder.GetAllConnections().FirstOrDefault(it => it.IsTo(node, resultInputPort));
            if (existingConnection != null)
            {
                Holder.RemoveConnection(existingConnection);
                Holder.AddConnection(existingConnection.From.Id, existingConnection.FromPort,
                    existingConnection.To.Id, node.NestLevel);
            }
        }
    }
}