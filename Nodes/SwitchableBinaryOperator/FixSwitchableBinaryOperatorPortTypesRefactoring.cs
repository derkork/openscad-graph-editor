using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    public class FixSwitchableBinaryOperatorPortTypesRefactoring : NodeRefactoring
    {
        public override bool IsLate => true;

        /// <summary>
        /// The input port that has been changed.
        /// </summary>
        private readonly int _inputPort;

        public FixSwitchableBinaryOperatorPortTypesRefactoring(ScadGraph holder, ScadNode node, int inputPort) : base(holder, node)
        {
            _inputPort = inputPort;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            
        }
    }
}