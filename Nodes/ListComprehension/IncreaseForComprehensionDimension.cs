using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes.ListComprehension;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.ForComprehension
{
    [UsedImplicitly]
    public class IncreaseForComprehensionDimension : UserSelectableNodeRefactoring
    {
        public override string Title => "Increase dimension";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is ForComprehensionStart;

        public IncreaseForComprehensionDimension(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            ((ForComprehensionStart) Node).IncreaseNestLevel();
        }
    }
}