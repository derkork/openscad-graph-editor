using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class RemoveCommentAction : IEditorAction
    {
        public int Order => 2100;
        public string Group => "";

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node))
            {
                if (node is Comment || node.TryGetComment(out _))
                {
                    result = new QuickAction("Remove comment", () => 
                        context.PerformRefactoring("Remove comment", 
                            new ChangeCommentRefactoring(graph, node, "", "")));
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}