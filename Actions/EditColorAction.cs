using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class EditColorAction : DebuggingAidsAction
    {
        public override int Order => 5000;

        public override bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node) && node is ICanHaveModifier)
            {
                result = new QuickAction("Set color", () => context.EditNodeColor(graph, node));
                return true;
            }

            result = default;
            return false;
        }
    }
}