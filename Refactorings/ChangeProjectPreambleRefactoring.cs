namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeProjectPreambleRefactoring : Refactoring
    {
        private readonly string _newPreamble;
        
        public ChangeProjectPreambleRefactoring(string newPreamble)
        {
            _newPreamble = newPreamble;
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            context.Project.Preamble = _newPreamble;
        }
    }
}