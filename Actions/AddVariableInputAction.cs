using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class AddVariableInputAction : IEditorAction
    {
        public int Order => 0;
        public string Group => "Input Ports";
        
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node)
                && node is IHaveVariableInputSize variableInputSize)
            {
                var title = variableInputSize.AddRefactoringTitle;
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title, new AddVariableInputRefactoring(graph, node)));
                return true;
            }
            result = default;
            return false;
        }
    }
}