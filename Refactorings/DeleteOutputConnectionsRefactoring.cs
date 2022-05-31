using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DeleteOutputConnectionsRefactoring : NodeRefactoring
    {
        private readonly int _port;

        public DeleteOutputConnectionsRefactoring(ScadGraph holder, ScadNode node, int port) : base(holder, node)
        {
            _port = port;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            Holder.GetAllConnections()
                .Where(it => it.From.Id == Node.Id && it.FromPort == _port)
                .ToList()
                .ForAll(Holder.RemoveConnection);
        }
    }
}