using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DeleteVariableRefactoring : Refactoring
    {
        private readonly VariableDescription _description;

        public DeleteVariableRefactoring(VariableDescription description)
        {
            _description = description;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // find all nodes that somehow refer to this variable
            context.Project.FindAllReferencingNodes(_description)
                // create a deletion refactoring for them
                .Select(it => new DeleteNodeRefactoringSimple(it.Graph, it.Node) )
                .ToList() // avoid modifying the project while still reading it
                // and perform the deletion refactorings
                .ForAll(context.PerformRefactoring);
            
            // finally delete the variable entry from the project
            context.Project.RemoveVariable(_description);
        }
    }
}