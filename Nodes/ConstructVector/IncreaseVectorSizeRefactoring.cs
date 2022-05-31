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
        
        public IncreaseVectorSizeRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            ((ConstructVector) Node).IncreaseVectorSize();
        }
    }
}