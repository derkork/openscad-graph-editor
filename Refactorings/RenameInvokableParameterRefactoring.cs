using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class RenameInvokableParameterRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;
        private readonly string _oldParameterName;
        private readonly string _newParameterName;

        public RenameInvokableParameterRefactoring(InvokableDescription description, string oldParameterName,
            string newParameterName)
        {
            _description = description;
            _oldParameterName = oldParameterName;
            _newParameterName = newParameterName;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // first find all graphs that are affected by this and make them refactorable
            // this will force a re-draw of all graphs that may be currently open.
            context.Project.FindContainingReferencesTo(_description)
                .ForAll(it => context.MakeRefactorable(it));
            
            // changing a parameter name is actually quite trivial, we just need to change it
            // in the description. 
            _description.Parameters.First(it => it.Name == _oldParameterName).Name = _newParameterName;
        }
    }
}