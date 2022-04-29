using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Max
{
    [UsedImplicitly]
    public class AddMinInputPortRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Add input port";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is Max;

        public AddMinInputPortRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (Max) graph.ById(Node.Id);
            
            // this is really simple just add a new input.
            node.AddInput();
        }
    }
}