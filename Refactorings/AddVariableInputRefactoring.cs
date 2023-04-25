using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring for nodes which implement <see cref="IHaveVariableInputSize"/>. This adds a new input to the node.
    /// </summary>
    [UsedImplicitly]
    public class AddVariableInputRefactoring : NodeRefactoring
    {

        public AddVariableInputRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (!(Node is IHaveVariableInputSize variableInputSize))
            {
                return; // not applicable
            }
            variableInputSize.AddVariableInputPort();
        }
    }
}