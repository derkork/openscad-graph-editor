using JetBrains.Annotations;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class ShowHelpAction : IEditorAction
    {
        public int Order => 99999;
        public string Group => "";
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node))
            {
                result = new QuickAction($"Show help",
                    () => context.ShowHelp(graph, node));
                return true;
            }

            result = default;
            return false;
        }
    }
}