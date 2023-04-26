using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    public class SwitchBinaryOperatorInputPortTypesRefactoring : NodeRefactoring
    {
        private readonly PortType _firstPortType;
        private readonly PortType _secondPortType;

        public SwitchBinaryOperatorInputPortTypesRefactoring(ScadGraph holder, SwitchableBinaryOperator node, PortType firstPortType, PortType secondPortType) : base(holder, node)
        {
            _firstPortType = firstPortType;
            _secondPortType = secondPortType;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var switchableBinaryOperator = (SwitchableBinaryOperator) Node;
            
            // if the port types are already as desired, we don't need to do anything
            if (switchableBinaryOperator.GetPortType(PortId.Input(0)) == _firstPortType &&
                switchableBinaryOperator.GetPortType(PortId.Input(1)) == _secondPortType)
            {
                return;
            }
            
            switchableBinaryOperator.SwitchInputPorts(_firstPortType, _secondPortType);
            
            // switching the input ports may affect the output port type, so we need to check
            // if all currently active connections are still valid.
            
            // first check connection from the first input port
            var deleteInputConnections1 = Holder.GetAllConnections()
                .Where(it => it.IsTo(Node, 0))
                .Where(it => ConnectionRules.CanConnect(it, new ConnectionRuleEvaluationFlags(true))
                    .Decision == ConnectionRules.OperationRuleDecision.Veto)
                .Select(it => new DeleteConnectionRefactoring(it));

            // now second input port
            var deleteInputConnections2 = Holder.GetAllConnections()
                .Where(it => it.IsTo(Node, 1))
                .Where(it => ConnectionRules.CanConnect(it, new ConnectionRuleEvaluationFlags(true))
                    .Decision == ConnectionRules.OperationRuleDecision.Veto)
                .Select(it => new DeleteConnectionRefactoring(it));
                
            // and the output port
            var deleteOutputConnections = Holder.GetAllConnections()
                .Where(it => it.IsFrom(Node, 0))
                .Where(it => ConnectionRules.CanConnect(it, new ConnectionRuleEvaluationFlags(true))
                    .Decision == ConnectionRules.OperationRuleDecision.Veto)
                .Select(it => new DeleteConnectionRefactoring(it));
                
            // union all the connections to delete and perform them
            var allConnectionsToDelete = deleteInputConnections1
                .Union(deleteInputConnections2)
                .Union(deleteOutputConnections);
            
            foreach (var deleteConnectionRefactoring in allConnectionsToDelete)
            {
                context.PerformRefactoring(deleteConnectionRefactoring);
            }
          
        }
    }
}