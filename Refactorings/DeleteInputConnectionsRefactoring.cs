using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DeleteInputConnectionsRefactoring : NodeRefactoring
    {
        private readonly int _port;

        public DeleteInputConnectionsRefactoring(ScadGraph holder, ScadNode node, int port) : base(holder, node)
        {
            _port = port;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            Holder.GetAllConnections()
                .Where(it => it.To.Id == Node.Id && it.ToPort == _port)
                .ToList()
                .ForAll(Holder.RemoveConnection);
        }
    }
}