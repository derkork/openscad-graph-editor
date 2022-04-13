using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactorings
{
    public class IntroduceInvokableRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;

        public IntroduceInvokableRefactoring(InvokableDescription description)
        {
            _description = description;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            context.Project.AddInvokable(_description);
        }
    }
}