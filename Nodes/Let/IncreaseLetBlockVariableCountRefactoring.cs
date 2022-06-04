using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Let
{
    [UsedImplicitly]
    public class IncreaseLetBlockVariableCountRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Add variable";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is LetBlock;

        public IncreaseLetBlockVariableCountRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (LetBlock) Node;
            // when increasing nest level, we need to fix the connection to the "After" port, as it moves one port down.
            var afterPort = node.VariableCount + 1;

            node.IncreaseVariableCount();
            
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