using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DropInputConnectionsRefactoring : NodeRefactoring
    {
        private readonly int _port;

        public DropInputConnectionsRefactoring(IScadGraph holder, ScadNode node, int port) : base(holder, node)
        {
            _port = port;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var refactorableGraph = context.MakeRefactorable(Holder);
            refactorableGraph.GetAllConnections()
                .Where(it => it.To.Id == Node.Id && it.ToPort == _port)
                .ToList()
                .ForAll(refactorableGraph.RemoveConnection);
        }
    }
}