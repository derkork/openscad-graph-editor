using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactoring
{
    public class DecreaseLoopNestLevelRefactoring : NodeRefactoring
    {
        public override string Title => "Decrease Loop Nest Level";

        public override bool Applies => Node is ForLoop forLoop && forLoop.NestLevel > 1;

        public DecreaseLoopNestLevelRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(ScadProject project)
        {
            var graph = MakeRefactorable(Holder, project);
            var node = (ForLoop) graph.ById(Node.Id);

            // when decreasing nest level, we loose connections (incoming and outgoing)
            // incoming connections
            graph.GetAllConnections()
                .Where(it => it.IsTo(node, node.NestLevel))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => graph.Remove(it));
            
            // outgoing connections
            graph.GetAllConnections()
                .Where(it => it.IsFrom(node, node.NestLevel))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => graph.Remove(it));
            
            
            node.DecreaseNestLevel();
            
            TransferModifiedDataBackToVisibleGraphs(project);            
        }
    }
}