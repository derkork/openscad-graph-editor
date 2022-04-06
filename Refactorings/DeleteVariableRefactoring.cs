using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class DeleteVariableRefactoring : Refactoring
    {
        private readonly VariableDescription _description;

        public DeleteVariableRefactoring(VariableDescription description)
        {
            _description = description;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // find all graphs that refer to this variable and make them refactorable
            var graphs = context.Project.FindContainingReferencesTo(_description)
                .Select(context.MakeRefactorable)
                .ToList();
            
            // now walk all graphs and remove all nodes that refer to this variable
            foreach (var graph in graphs)
            {
                var nodesToKill = graph.GetAllNodes()
                    .Where(it =>
                        it is IReferToAVariable iReferToAVariable &&
                        iReferToAVariable.VariableDescription == _description)
                    .ToList();

                // first kill all connections involving any of these nodes from the graph
                graph.GetAllConnections().Where(it => nodesToKill.Any(it.InvolvesNode))
                    .ToList()
                    .ForAll(it => graph.RemoveConnection(it));
                
                // then kill the nodes themselves
                nodesToKill.ForAll(it => graph.RemoveNode(it));
            }
            
            // finally delete the variable entry from the project
            context.Project.RemoveVariable(_description);
        }
    }
}