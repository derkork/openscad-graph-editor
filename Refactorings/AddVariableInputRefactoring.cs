using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring for nodes which implement <see cref="IHaveVariableInputSize"/>. This adds a new input to the node.
    /// </summary>
    [UsedImplicitly]
    public class AddVariableInputRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => ((IHaveVariableInputSize) Node).AddRefactoringTitle;
        public override bool IsApplicableToNode => Node is IHaveVariableInputSize;

        public override int Order => 0;

        public AddVariableInputRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var node = (IHaveVariableInputSize) Node;
            node.AddVariableInputPort();
        }
    }
}