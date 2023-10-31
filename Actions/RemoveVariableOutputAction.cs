using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class RemoveVariableOutputAction : IEditorAction
    {
        public int Order => 1;
        public string Group => "Output Ports";
        
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node)
                && node is IHaveVariableOutputSize variableOutputSize
                && variableOutputSize.CurrentOutputSize > 1)
            {
                var title = variableOutputSize.RemoveRefactoringTitle;
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title, new RemoveVariableOutputRefactoring(graph, node)));
                return true;
            }
            result = default;
            return false;
        }
    }
}