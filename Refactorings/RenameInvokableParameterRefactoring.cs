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
            if (_oldParameterName == _newParameterName)
            {
                return; // nothing to do
            }

            //  change it in the description. 
            _description.Parameters.First(it => it.Name == _oldParameterName).Name = _newParameterName;
            
            // now find all nodes which refer to it and set up their ports again so they pick up the updated name
            context.Project.FindAllReferencingNodes(_description)
                .ForAll(it => it.NodeAsReference.SetupPorts(_description));
            // we can leave the literals alone as they haven't changed.
        }
    }
}