using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Nodes.SwitchableBinaryOperator;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    [UsedImplicitly]
    public class FlipBinaryOperatorInputsAction : IEditorAction
    {
        public int Order => 10;
        public string Group => "";
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node, out _)
                && node is BinaryOperator && !(node is SwitchableBinaryOperator.SwitchableBinaryOperator))
            {
                const string title = "Flip inputs";
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title, new FlipBinaryOperatorInputsRefactoring(graph, node)));
                return true;
            }

            result = default;
            return false;
        }
    }
}