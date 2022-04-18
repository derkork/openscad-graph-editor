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
            
            // finally delete the graph defining the invokable itself.
            var definingGraph = context.Project.FindDefiningGraph(_description);
            GdAssert.That(definingGraph != null, "definingGraph != null");
            
            var refactorable = context.MakeRefactorable(definingGraph);
            context.Project.RemoveInvokable(_description);

            context.MarkDeleted(refactorable);
        }
    }
}