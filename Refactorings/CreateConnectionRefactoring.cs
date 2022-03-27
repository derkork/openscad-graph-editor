using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class CreateConnectionRefactoring : Refactoring
    {
        private readonly ScadConnection _connection;

        public CreateConnectionRefactoring(ScadConnection connection)
        {
            _connection = connection;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(_connection.Owner);
            graph.AddConnection(_connection.From.Id, _connection.FromPort, _connection.To.Id, _connection.ToPort);
        }
    }
}