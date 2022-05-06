using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Min
{
    [UsedImplicitly]
    public class AddMinInputPortRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Add input port";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is Min;

        public AddMinInputPortRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            // this is really simple just add a new input.
            ((Min)  Node).AddInput();
        }
    }
}