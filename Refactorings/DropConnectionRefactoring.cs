using System.Linq;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DropConnectionRefactoring : Refactoring
    {
        private readonly ScadConnection _connection;

        public DropConnectionRefactoring(ScadConnection connection)
        {
            _connection = connection;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(_connection.Owner);
            graph.GetAllConnections()
                .Where(it => _connection.RepresentsSameAs(it))
                .ToList()
                .ForAll(it => graph.RemoveConnection(it));
        }
    }
}