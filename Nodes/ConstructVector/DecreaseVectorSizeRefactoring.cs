using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ConstructVector
{
    [UsedImplicitly]
    public class DecreaseVectorSizeRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Remove Item";
        public override bool IsApplicableToNode => Node is ConstructVector indexVector && indexVector.VectorSize > 1;

        public override int Order => 1;
        
        public DecreaseVectorSizeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (ConstructVector) Node;

            var lastInputPort = node.VectorSize-1;
            
            // we now need to remove all connections that go to the last input port
            Holder.GetAllConnections()
                .Where(it => it.IsTo(node, lastInputPort))
                .ToList() // we need to copy the list because we are modifying it
                .ForAll(it => Holder.RemoveConnection(it));
            
            node.DecreaseVectorSize();
        }
    }
}