using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class EditCommentAction : IEditorAction
    {
        public int Order => 2000;
        public string Group => "";

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node))
            {
                var text = "Edit comment";

                if (!(node is Comment))
                {
                    if (!node.TryGetComment(out _))
                    {
                        text = "Add comment";
                    }
                }
                result = new QuickAction(text, () => context.EditComment(graph, node));
                return true;
            }

            result = default;
            return false;
        }
    }
}