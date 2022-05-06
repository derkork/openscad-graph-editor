using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    public class SwitchBinaryOperatorPortTypeRefactoring : UserSelectableNodeRefactoring
    {
        private readonly bool _isFirstOperand;
        private readonly PortType _targetPortType;

        public SwitchBinaryOperatorPortTypeRefactoring(IScadGraph holder, ScadNode node, bool isFirstOperand, PortType targetPortType) : base(holder, node)
        {
            _isFirstOperand = isFirstOperand;
            _targetPortType = targetPortType;
        }

        public override int Order => (_isFirstOperand ? 1 : 2) * (int) _targetPortType + 1;

        public override string Title => $"Switch to {_targetPortType.HumanReadableName()}";
        
        public override  string Group => $"{(_isFirstOperand ? "1st" : "2nd")} operand";
        
        public override bool IsApplicableToNode => Node is SwitchableBinaryOperator switchableBinaryOperator
                                                   && switchableBinaryOperator.Supports(_targetPortType)
                                                   && switchableBinaryOperator.GetPortType(PortId.Input(_isFirstOperand ? 0 : 1)) != _targetPortType;

        public override void PerformRefactoring(RefactoringContext context)
        {
            var switchableBinaryOperator = (SwitchableBinaryOperator) Node;
            if (_isFirstOperand)
            {
                switchableBinaryOperator.SwitchPortType(PortId.Input(0), _targetPortType);
            }
            else
            {
                switchableBinaryOperator.SwitchPortType(PortId.Input(1), _targetPortType);
            }
            
            // when we switch the port type it may make certain connections invalid. Therefore we will need to check.
            var portId = _isFirstOperand ? PortId.Input(0) : PortId.Input(1);


            var deleteRefactorings = Holder.GetAllConnections()
                // all connections to the port we are switching
                .Where(it => it.InvolvesPort(Node, portId))
                // check if they are still valid
                .Where(it => ConnectionRules.CanConnect(it).Decision == ConnectionRules.OperationRuleDecision.Veto)
                // create a refactoring for each invalid
                .Select(it => new DeleteConnectionRefactoring(it))
                .ToList();

            foreach (var deleteConnectionRefactoring in deleteRefactorings)
            {
                context.PerformRefactoring(deleteConnectionRefactoring);
            }
        }
    }
}