using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DeleteInvokableRefactoring : Refactoring
    {
        private readonly InvokableDescription _description;

        public DeleteInvokableRefactoring(InvokableDescription description)
        {
            _description = description;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // find all nodes that refer to this invokable and delete them including all connections
            context.Project.FindAllReferencingNodes(_description)
                .Select(it => new DeleteNodeRefactoringSimple(it.Graph, it.Node))
                .ToList() // avoid concurrent modification
                .ForAll(context.PerformRefactoring);
            
            // if the invokable is defined in the project, delete the graph
            if (context.Project.IsDefinedInThisProject(_description))
            {
                context.Project.RemoveInvokable(_description);
            }
            else
            {
                // invokable came from an external reference
                if (context.Project.TryGetExternalReferenceHolding(_description, out var externalReference))
                {
                    switch (_description)
                    {
                        case FunctionDescription _:
                            externalReference.Functions.Remove(
                                externalReference.Functions.First(it => it.Id == _description.Id));
                            break;
                        case ModuleDescription _:
                            externalReference.Modules.Remove(
                                externalReference.Modules.First(it => it.Id == _description.Id));
                            break;        
                    }
                }
                else
                {
                    GdAssert.That(false, "Could not find external reference holding the invokable");
                }
            }
        }
    }
}