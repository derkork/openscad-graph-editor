using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactorings
{
    public class RenameVariableRefactoring : Refactoring
    {
        private readonly VariableDescription _description;
        private readonly string _newName;

        public RenameVariableRefactoring(VariableDescription description, string newName)
        {
            _description = description;
            _newName = newName;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // change the name
            _description.Name = _newName;
        }
    }
}