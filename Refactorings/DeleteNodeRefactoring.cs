using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Deletes a node. The node must not have any connections.
    /// </summary>
    public class DeleteNodeRefactoring : NodeRefactoring
    {
        public DeleteNodeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var toDelete = graph.ById(Node.Id);
            graph.RemoveNode(toDelete);
        }
    }
}