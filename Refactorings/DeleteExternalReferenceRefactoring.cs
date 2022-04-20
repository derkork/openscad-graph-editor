using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DeleteExternalReferenceRefactoring : Refactoring
    {
        private readonly ExternalReference _toDelete;

        public DeleteExternalReferenceRefactoring(ExternalReference toDelete) 
        {
            _toDelete = toDelete;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // we have to clean up all references to anything that was pulled in from this external reference.

            // get all nodes  that refer to functions or modules in this external reference
            foreach (var invokableDescription in _toDelete.Functions.Concat<InvokableDescription>(
                         _toDelete.Modules))
            {
                context.Project
                    .FindAllReferencingNodes(invokableDescription)
                    .Select(it => new DeleteNodeRefactoringSimple(it.Graph, it.Node))
                    .ToList() // avoid still reading from the project while running the refactorings
                    .ForAll(context.PerformRefactoring);
            }
            
            // same for all variables
            foreach (var variable in _toDelete.Variables)
            {
                context.Project
                    .FindAllReferencingNodes(variable)
                    .Select(it => new DeleteNodeRefactoringSimple(it.Graph, it.Node))
                    .ToList() // avoid still reading from the project while running the refactorings
                    .ForAll(context.PerformRefactoring);
            }
            
            // and tell the project to forget about the external reference
            context.Project.RemoveExternalReference(_toDelete);
        }
    }
}