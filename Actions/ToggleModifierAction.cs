using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    public abstract class ToggleModifierAction : DebuggingAidsAction
    {
        public override bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node) && node is ICanHaveModifier)
            {
                result = BuildAction(context, graph, node);
                return true;
            }

            result = default;
            return false;
        }

        protected abstract QuickAction BuildAction(IEditorContext context, ScadGraph graph, ScadNode node);
    }
}