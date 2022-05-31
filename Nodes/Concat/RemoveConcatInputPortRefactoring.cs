using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Concat
{
    [UsedImplicitly]
    public class RemoveConcatInputPortRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Remove input port";
        public override int Order => 1;
        public override bool IsApplicableToNode => Node is Concat concat && concat.InputPortCount > 1;

        public RemoveConcatInputPortRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {

            // remove the connection that goes into the port to be removed.
            Holder.GetAllConnections()
                .Where(it => it.IsTo(Node, Node.InputPortCount))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => Holder.RemoveConnection(it));
            
            ((Concat)Node).RemoveInput();
        }
    }
}