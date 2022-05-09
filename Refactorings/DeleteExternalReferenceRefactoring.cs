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
            // Prepare the reference for removal. This function will return all
            // the removed reference and all transitive references that were removed as well
            var allRemovedReferences = context.Project.PrepareForRemoval(_toDelete).ToList();

            // now we need to delete everything that refers to something inside those deleted references
            foreach (var reference in allRemovedReferences)
            {
                // get all nodes  that refer to functions or modules in this external reference
                foreach (var invokableDescription in reference.Functions.Concat<InvokableDescription>(
                             reference.Modules))
                {
                    context.Project
                        .FindAllReferencingNodes(invokableDescription)
                        .Select(it => new DeleteNodeRefactoring(it.Graph, it.Node))
                        .ToList() // avoid still reading from the project while running the refactorings
                        .ForAll(context.PerformRefactoring);
                }

                // same for all variables
                foreach (var variable in reference.Variables)
                {
                    context.Project
                        .FindAllReferencingNodes(variable)
                        .Select(it => new DeleteNodeRefactoring(it.Graph, it.Node))
                        .ToList() // avoid still reading from the project while running the refactorings
                        .ForAll(context.PerformRefactoring);
                }
            }
            
            // finally remove the references from the project
            allRemovedReferences.ForAll(it => context.Project.RemoveExternalReference(it));
        }
    }
}