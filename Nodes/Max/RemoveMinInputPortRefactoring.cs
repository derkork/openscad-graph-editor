using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Max
{
    [UsedImplicitly]
    public class RemoveMinInputPortRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Remove input port";
        public override int Order => 1;
        public override bool IsApplicableToNode => Node is Max echo && echo.InputPortCount > 1;

        public RemoveMinInputPortRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (Max) graph.ById(Node.Id);

            // remove the connection that goes into the port to be removed.
            graph.GetAllConnections()
                .Where(it => it.IsTo(node, node.InputPortCount))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => graph.RemoveConnection(it));
            
            node.RemoveInput();
        }
    }
}