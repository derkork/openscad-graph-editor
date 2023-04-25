using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class RemoveColorAction : DebuggingAidsAction
    {
        public override int Order => 5100;

        public override bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.TryGetNode(out var graph, out var node, out _)
                && node.GetModifiers().HasFlag(ScadNodeModifier.Color))
            {
                result = new QuickAction("Clear color",
                    () => context.PerformRefactoring("Remove color",
                        new ToggleModifierRefactoring(graph, node, ScadNodeModifier.Color, false)));
                return true;
            }

            result = default;
            return false;
        }
    }
}