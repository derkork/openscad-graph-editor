using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Echo
{
    [UsedImplicitly]
    public class AddEchoInputPortRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Add input port";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is Echo;

        public AddEchoInputPortRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (Echo) graph.ById(Node.Id);
            
            // this is really simple just add a new input.
            node.AddInput();
        }
    }
}