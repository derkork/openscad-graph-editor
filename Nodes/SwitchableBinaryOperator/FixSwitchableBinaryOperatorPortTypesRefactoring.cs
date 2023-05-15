using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    public class FixSwitchableBinaryOperatorPortTypesRefactoring : NodeRefactoring
    {
        public override bool IsLate => true;

        public FixSwitchableBinaryOperatorPortTypesRefactoring(ScadGraph holder, SwitchableBinaryOperator node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // Plan of attack:
            // We first need to find out which inputs currently go into the node. We then switch the input
            // ports to have the correct type. If an input port is currently not connected, we switch it to the
            // same type as the other. The node should automatically switch to the correct
            // its output type based on the current input types. Finally we need to check if the output
            // connection is still valid. If not, we need to disconnect it.
            
            var node = (SwitchableBinaryOperator) Node;
            
            // first we need to find out which inputs currently go into the node
            var firstIsConnected = Holder.GetAllConnections()
                .Any(it => it.IsTo(Node, 0));
            
            var firstPortType = Holder.GetAllConnections()
                .Where(it => it.IsTo(Node, 0))
                .Select(it => it.From.GetPortType(PortId.Output(it.FromPort)))
                .DefaultIfEmpty(PortType.Any)
                .First();
            
            
            var secondIsConnected = Holder.GetAllConnections()
                .Any(it => it.IsTo(Node, 1));
            
            var secondPortType = Holder.GetAllConnections()
                .Where(it => it.IsTo(Node, 1))
                .Select(it => it.From.GetPortType(PortId.Output(it.FromPort)))
                .DefaultIfEmpty(PortType.Any)
                .First();
            
            
            // if one input is connected and the other is not, we need to switch the unconnected input to the same type
            if (firstIsConnected && !secondIsConnected)
            {
                secondPortType = firstPortType;
            }
            else if (!firstIsConnected && secondIsConnected)
            {
                firstPortType = secondPortType;
            }
            
            // now we switch the input ports to have the correct type, this will also fix any broken connections
            context.PerformRefactoring(new SwitchBinaryOperatorInputPortTypesRefactoring(Holder, node, firstPortType, secondPortType));
        }
    }
}