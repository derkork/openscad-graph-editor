using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class RemoveVariableInputAction : IEditorAction
    {
        public int Order => 1;
        public string Group => "Input Ports";
        
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node)
                && node is IHaveVariableInputSize variableInputSize
                && variableInputSize.CurrentInputSize > 1)
            {
                var title = variableInputSize.RemoveRefactoringTitle;
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title, new RemoveVariableInputRefactoring(graph, node)));
                return true;
            }
            result = default;
            return false;
        }
    }
}