using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    [UsedImplicitly]
    public class IncreaseVectorSizeRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Add Item";
        public override bool IsApplicableToNode => Node is ConstructVector;

        public override int Order => 0;
        
        public IncreaseVectorSizeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (ConstructVector) graph.ById(Node.Id);

            node.IncreaseVectorSize();
        }
    }
}