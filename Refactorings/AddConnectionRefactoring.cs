using Godot;
using OpenScadGraphEditor.Nodes;
using Serilog;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring to create a connection. Will check if the connection can be made according to the
    /// connection rules and will also perform all side effects that are required for this connection
    /// to be made.
    /// </summary>
    public class AddConnectionRefactoring : Refactoring
    {
        private readonly ScadConnection _connection;

        public AddConnectionRefactoring(ScadConnection connection)
        {
            _connection = connection;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var operationResult = ConnectionRules.CanConnect(_connection);

            if (operationResult.Decision == ConnectionRules.OperationRuleDecision.Veto)
            {
                Log.Information("Connection vetoed");
                return;
            }
            
            // first run all the side effects
            foreach(var sideEffect in operationResult.Refactorings)
            {
                context.PerformRefactoring(sideEffect);
            }
            
            // then create the connection
            var graph =_connection.Owner;
            graph.AddConnection(_connection.From.Id, _connection.FromPort, _connection.To.Id, _connection.ToPort);
        }
    }
}