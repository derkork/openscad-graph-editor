using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes.SwitchableBinaryOperator
{
    [UsedImplicitly]
    public class FlipSwitchableBinaryOperatorInputsAction : IEditorAction
    {
        public int Order => 10;
        public string Group => "";
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node)
                && node is SwitchableBinaryOperator)
            {
                const string title = "Flip inputs";
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title, new FlipSwitchableBinaryOperatorInputsRefactoring(graph, node)));
                return true;
            }

            result = default;
            return false;
        }
    }
}