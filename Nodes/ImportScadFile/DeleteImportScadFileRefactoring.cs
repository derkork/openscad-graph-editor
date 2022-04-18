using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ImportScadFile
{
    public class DeleteImportScadFileRefactoring : NodeRefactoring
    {
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        public DeleteImportScadFileRefactoring(IScadGraph holder, ImportScadFile node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (ImportScadFile) Node;
            var externalReference = node.ExternalReference;

            var anyOtherReference = context.Project
                .FindAllReferencingNodes(externalReference)
                .Any(it => it.Node.Id != node.Id);

            if (anyOtherReference)
            {
                // there are still other nodes that reference this external reference, so we can just delete the import
                // and leave everything else alone
                context.PerformRefactoring(new DeleteNodeRefactoringSimple(Holder, Node));
                return;
            }


            // if the node count is zero, we are about to delete the last include for this reference and therefore
            // have to clean up all references to anything that was pulled in from this external reference.

            // get all nodes  that refer to functions or modules in this external reference
            foreach (var invokableDescription in externalReference.Functions.Concat<InvokableDescription>(
                         externalReference.Modules))
            {
                context.Project
                    .FindAllReferencingNodes(invokableDescription)
                    .Select(it => new DeleteNodeRefactoringSimple(it.Graph, it.Node))
                    .ToList() // avoid still reading from the project while running the refactorings
                    .ForAll(context.PerformRefactoring);
            }
            
            // same for all variables
            foreach (var variable in externalReference.Variables)
            {
                context.Project
                    .FindAllReferencingNodes(variable)
                    .Select(it => new DeleteNodeRefactoringSimple(it.Graph, it.Node))
                    .ToList() // avoid still reading from the project while running the refactorings
                    .ForAll(context.PerformRefactoring);
            }
            
            // and finally delete the node itself
            context.PerformRefactoring(new DeleteNodeRefactoringSimple(Holder, Node));
            
            // and tell the project to forget about the external reference
            context.Project.RemoveExternalReference(externalReference);
        }
    }
}