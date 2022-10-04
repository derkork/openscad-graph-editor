using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeVariableDocumentationRefactoring : Refactoring
    {
        private readonly VariableDescription _description;
        private readonly string _newDescription;

        public ChangeVariableDocumentationRefactoring(VariableDescription description, string newDescription)
        {
            _description = description;
            _newDescription = newDescription;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // change the description
            _description.Description = _newDescription;
        }
    }
}