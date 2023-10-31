using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring for nodes which implement <see cref="IHaveVariableOutputSize"/>. This adds a new output to the node.
    /// </summary>
    [UsedImplicitly]
    public class AddVariableOutputRefactoring : NodeRefactoring
    {

        public AddVariableOutputRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (!(Node is IHaveVariableOutputSize iHaveVariableOutputSize))
            {
                return; // not applicable
            }
            iHaveVariableOutputSize.AddVariableOutputPort();
        }
    }
}