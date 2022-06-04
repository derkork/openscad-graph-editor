using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Let
{
    [UsedImplicitly]
    public class DecreaseLetBlockVariableCountRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Remove variable";
        public override int Order => 1;
        public override bool IsApplicableToNode => Node is LetBlock letBlock && letBlock.VariableCount > 1;

        public DecreaseLetBlockVariableCountRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (LetBlock) Node;

            // when decreasing nest level, we loose connections (incoming and outgoing)
            // incoming connections
            Holder.GetAllConnections()
                .Where(it => it.IsTo(node, node.VariableCount))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => Holder.RemoveConnection(it));
            
            // outgoing connections
            Holder.GetAllConnections()
                .Where(it => it.IsFrom(node, node.VariableCount))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => Holder.RemoveConnection(it));
            
            
            node.DecreaseVariableCount();
        }
    }
}