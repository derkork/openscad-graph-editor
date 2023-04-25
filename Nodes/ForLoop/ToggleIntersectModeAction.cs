using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes.ForLoop
{
    [UsedImplicitly]
    public class ToggleIntersectModeAction : IEditorAction
    {
        public int Order => 0;
        public string Group => "";

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node, out _)
                && node is ForLoopStart forLoopStart)
            {
                const string title = "Toggle intersect mode";
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title, new ToggleIntersectModeRefactoring(graph, forLoopStart)));
                return true;
            }

            result = default;
            return false;
        }
    }
}