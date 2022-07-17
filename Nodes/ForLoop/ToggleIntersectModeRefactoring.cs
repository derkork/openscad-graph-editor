using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.ForLoop
{

    /// <summary>
    /// Refactoring to toggle the intersect mode for the for loop.
    /// </summary>
    [UsedImplicitly]
    public class ToggleIntersectModeRefactoring :  UserSelectableNodeRefactoring
    {
        public override string Title => "Toggle intersect mode";
        public override bool IsApplicableToNode => Node is ForLoopStart;
        public override int Order => 0;

        public ToggleIntersectModeRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var forLoopStart = (ForLoopStart)Node;  
            // this has no impact on connection it just changes the rendered text, so this is all we need to do
            forLoopStart.IntersectMode = !forLoopStart.IntersectMode;
        }
    }
}