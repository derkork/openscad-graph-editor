using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes.ListComprehension;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ForComprehension
{
    [UsedImplicitly]
    public class DecreaseForComprehensionDimension : UserSelectableNodeRefactoring
    {
        public override string Title => "Decrease dimension";
        public override int Order => 1;
        public override bool IsApplicableToNode => Node is ForComprehensionStart forComprehension && forComprehension.NestLevel > 1;

        public DecreaseForComprehensionDimension(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = Holder;
            var node = (ForComprehensionStart) Node;
            
            // when decreasing nest level, we loose connections (incoming and outgoing)
            // incoming connections
            graph.GetAllConnections()
                .Where(it => it.IsTo(node, node.NestLevel-1))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => graph.RemoveConnection(it));
            
            // outgoing connections
            graph.GetAllConnections()
                .Where(it => it.IsFrom(node, node.NestLevel))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => graph.RemoveConnection(it));
            
            node.DecreaseNestLevel();
        }
    }
}