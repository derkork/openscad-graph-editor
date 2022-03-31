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
            // make all graphs refactorable that contain a reference to the given invokable. This will force
            // a redraw of them in case they were currently open for editing.
            context.Project.FindContainingReferencesTo(_description).ForAll(it => context.MakeRefactorable(it));
            
            // change the name
            _description.Name = _newName;
        }
    }
}