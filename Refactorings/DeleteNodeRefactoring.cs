using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Deletes a node. Will delete all connections from or to that node as well. Nodes that are marked as
    /// <see cref="ICannotBeDeleted"/> will not be deleted. Nodes marked with <see cref="IHaveSpecialDestruction"/>
    /// will be deleted using their destruction refactoring.
    /// </summary>
    public class DeleteNodeRefactoring : NodeRefactoring
    {
        public DeleteNodeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            switch (Node)
            {
                case ICannotBeDeleted _:
                    return;
                case IHaveSpecialDestruction haveSpecialDestruction:
                    var refactoring = haveSpecialDestruction.GetDestructionRefactoring(Holder);
                    context.PerformRefactoring(refactoring);
                    return;
            }


            // otherwise just call the simple destruction refactoring
            var simpleDeletion = new DeleteNodeRefactoringSimple(Holder, Node);
            context.PerformRefactoring(simpleDeletion);
        }
    }
}