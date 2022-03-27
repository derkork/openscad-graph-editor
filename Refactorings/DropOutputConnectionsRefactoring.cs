using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DropOutputConnectionsRefactoring : NodeRefactoring
    {
        private readonly int _port;

        public DropOutputConnectionsRefactoring(IScadGraph holder, ScadNode node, int port) : base(holder, node)
        {
            _port = port;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var refactorableGraph = context.MakeRefactorable(Holder);
            refactorableGraph.GetAllConnections()
                .Where(it => it.From.Id == Node.Id && it.FromPort == _port)
                .ToList()
                .ForAll(refactorableGraph.RemoveConnection);
        }

    }
}