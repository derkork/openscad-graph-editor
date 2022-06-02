using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.ForLoop
{
    [UsedImplicitly]
    public class IncreaseLoopNestLevelRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Increase Loop Nest Level";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is ForLoopStart;

        public IncreaseLoopNestLevelRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (ForLoopStart) Node;
            node.IncreaseNestLevel();
        }
    }
}