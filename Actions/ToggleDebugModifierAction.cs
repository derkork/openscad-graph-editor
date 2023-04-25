using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public abstract class ToggleDebugModifierAction : ToggleModifierAction
    {
        public override int Order => 1000;

        protected override QuickAction BuildAction(IEditorContext context, ScadGraph graph, ScadNode node)
        {
            var hasDebug = node.GetModifiers().HasFlag(ScadNodeModifier.Debug);
            return new QuickAction("Debug subtree",
                () => context.PerformRefactoring("Toggle: Debug subtree",
                    new ToggleModifierRefactoring(graph, node, ScadNodeModifier.Debug, !hasDebug)), true,
                hasDebug);
        }
    }
}