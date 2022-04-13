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
    public class DropConnectionRefactoring : Refactoring
    {
        private readonly ScadConnection _connection;

        public DropConnectionRefactoring(ScadConnection connection)
        {
            _connection = connection;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // it is important that we make the graph refactorable no matter what, as we need to re-draw the graph in any case
            // to synchronize the connection state with the visual graph.
            // I don't like to have this side effect here but I don't know how to do it better right now
            var graph = context.MakeRefactorable(_connection.Owner);

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