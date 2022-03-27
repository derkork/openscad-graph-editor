using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.IndexVector
{
    [UsedImplicitly]
    public class IncreaseVectorIndexPortsRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Add Port";
        public override bool IsApplicableToNode => Node is IndexVector;

        public override int Order => 0;
        
        public IncreaseVectorIndexPortsRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (IndexVector) graph.ById(Node.Id);

            node.IncreasePorts();
        }
    }
}