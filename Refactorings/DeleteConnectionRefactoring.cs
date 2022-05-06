using System.Linq;
using Godot;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Drops a connection. Verifies against the connection rules that this connection can actually be dropped.
    /// If it cannot be dropped, the connection stays.
    /// </summary>
    public class DeleteConnectionRefactoring : Refactoring
    {
        private readonly ScadConnection _connection;

        public DeleteConnectionRefactoring(ScadConnection connection)
        {
            _connection = connection;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = _connection.Owner;

            var result = ConnectionRules.CanDisconnect(_connection);
            if (result.Decision == ConnectionRules.OperationRuleDecision.Veto)
            {
                GD.Print("Disconnect was vetoed.");
                return; // nothing to do.
            }
            
            // first run all the side effects
            result.Refactorings.ForAll(context.PerformRefactoring);
            
            // then remove the connection
            graph.GetAllConnections()
                .Where(it => _connection.RepresentsSameAs(it))
                .ToList()
                .ForAll(it => graph.RemoveConnection(it));
        }
    }
}