using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactorings
{
    public class IntroduceVariableRefactoring : Refactoring
    {
        private readonly VariableDescription _description;

        public IntroduceVariableRefactoring(VariableDescription description)
        {
            _description = description;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            context.Project.AddVariable(_description);
        }
    }
}