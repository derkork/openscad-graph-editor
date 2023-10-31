using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class AddVariableOutputAction : IEditorAction
    {
        public int Order => 0;
        public string Group => "Output Ports";
        
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node)
                && node is IHaveVariableOutputSize variableOutputSize)
            {
                var title = variableOutputSize.AddRefactoringTitle;
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title, new AddVariableOutputRefactoring(graph, node)));
                return true;
            }
            result = default;
            return false;
        }
    }
}