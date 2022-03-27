using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.IndexVector
{
    [UsedImplicitly]
    public class DecreaseVectorIndexPortsRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Remove Port";
        public override bool IsApplicableToNode => Node is IndexVector indexVector && indexVector.IndexPortCount > 1;

        public override int Order => 1;
        
        public DecreaseVectorIndexPortsRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            var node = (IndexVector) graph.ById(Node.Id);

            var lastInputPort = node.IndexPortCount;
            var lastOutputPort = node.IndexPortCount - 1;
            
            // we now need to remove all connections that go to the last input or output port
            graph.GetAllConnections()
                .Where(it => it.IsFrom(node, lastOutputPort) || it.IsTo(node, lastInputPort))
                .ForAll(it => graph.RemoveConnection(it));
            
            node.DecreasePorts();
        }
    }
}