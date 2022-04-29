using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Concat
{
    [UsedImplicitly]
    public class AddConcatInputPortRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Add input port";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is Concat;

        public AddConcatInputPortRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (Concat) graph.ById(Node.Id);
            
            // this is really simple just add a new input.
            node.AddInput();
        }
    }
}