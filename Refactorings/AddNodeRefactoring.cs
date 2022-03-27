using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Adds a new node to the graph.
    /// </summary>
    public class AddNodeRefactoring : NodeRefactoring
    {
        public AddNodeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            graph.AddNode(Node);
        }
    }
}