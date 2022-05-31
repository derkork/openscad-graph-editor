using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Min
{
    [UsedImplicitly]
    public class AddMinInputPortRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => "Add input port";
        public override int Order => 0;
        public override bool IsApplicableToNode => Node is Min;

        public AddMinInputPortRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            // min changes its port types depending on how many inputs it has. we therefore need to 
            // get all incoming connections and save them. After we have added the input we need to
            // disconnect all connections that no longer fit.
            var connections = Holder.GetAllConnections()
                .Where(it => it.To == Node)
                .ToList();
            
            ((Min) Node).AddInput();
            
            foreach (var connection in connections
                         .Where(connection => ConnectionRules.CanConnect(connection).Decision ==ConnectionRules.OperationRuleDecision.Veto))
            {
                // delete the connection
                context.PerformRefactoring(new DeleteConnectionRefactoring(connection));
            }
        }
    }
}