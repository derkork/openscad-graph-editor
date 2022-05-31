using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.ForComprehension
{
    [UsedImplicitly]
    public class DecreaseForComprehensionDimension : UserSelectableNodeRefactoring
    {
        public override string Title => "Decrease dimension";
        public override int Order => 1;
        public override bool IsApplicableToNode => Node is ForComprehension forComprehension && forComprehension.NestLevel > 1;

        public DecreaseForComprehensionDimension(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = Holder;
            var node = (ForComprehension) Node;
            
            // the result port will move one level up, so we need to save the connection that goes into this port
            var resultConnection = graph.GetAllConnections()
                .FirstOrDefault(it => it.InvolvesPort(node, PortId.Input(node.NestLevel)));

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
            
            // drop the result connection
            if (resultConnection != null)
            {
                graph.RemoveConnection(resultConnection);
            }

            node.DecreaseNestLevel();
            
            // and re-build it.
            if (resultConnection != null)
            {
                graph.AddConnection(resultConnection.From.Id, resultConnection.FromPort, node.Id, node.NestLevel);
            }
        }
    }
}