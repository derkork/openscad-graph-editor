using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Sum
{
    public class FixSumPortTypesRefactoring : NodeRefactoring
    {
        public FixSumPortTypesRefactoring(ScadGraph owner, Sum sum) : base(owner, sum)
        {
        }

        public override bool IsLate => true;

        public override void PerformRefactoring(RefactoringContext context)
        {
            var sum = Node as Sum;
            // find all connections going into the sum
            var incomingConnections = Holder
                .GetAllConnections()
                .Where(it => ScadConnectionExt.IsTo(it, sum, 0))
                .ToList();
            
         
            
            // now determine the new port type.
            
            
            // this is the default
            // if any source port type is "Any" then the sum port type is "Any"
            // if we have mixed types, then the sum port type is "Any" (though this should not happen)
            var portType = PortType.Any;

            if (incomingConnections.Count > 0)
            {
                var incomingPortTypes = incomingConnections
                    .Select(it => it.TryGetFromPortType(out var result) ? result : PortType.Any)
                    .ToList();

                // if all source port types are "Number" then the sum port type is "Number"
                if (incomingPortTypes.All(it => it == PortType.Number))
                {
                    portType = PortType.Number;
                }
                
                // if all of the source types are some vector type, we use the largest vector type
                if (incomingPortTypes.All(it => it.CanBeAssignedTo(PortType.Vector)))
                {
                    portType = incomingPortTypes.Aggregate(PortType.Vector2, (a, b) => a.CanBeAssignedTo(b) ? b : a );
                }
            }

            
            // update the port type
            sum.SwitchPortType(portType);
            
            // changing the port type may have invalidated the outgoing connections, so we remove-them and 
            // re-add them. this will also trigger any side-effects of the connection rules (which will make other
            // connected nodes re-evaluate their port types)
            var outputConnections = Holder.GetAllConnections()
                .Where(it => it.IsFrom(Node, 0))
                .ToList();
            
            // delete
            outputConnections.ForAll(it => context.PerformRefactoring(new DeleteConnectionRefactoring(it)));
            
            // re-add
            outputConnections.ForAll(it => context.PerformRefactoring(new AddConnectionRefactoring(it)));

        }
    }
}