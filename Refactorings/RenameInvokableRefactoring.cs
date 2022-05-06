using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class RenameInvokableRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private readonly string _newName;

        public RenameInvokableRefactoring(InvokableDescription description, string newName)
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