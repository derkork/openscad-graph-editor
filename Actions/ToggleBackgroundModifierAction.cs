using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class ToggleBackgroundModifierAction : ToggleModifierAction
    {
        public override int Order => 3000;

        protected override QuickAction BuildAction(IEditorContext context, ScadGraph graph, ScadNode node)
        {
            var hasBackground = node.GetModifiers().HasFlag(ScadNodeModifier.Background);
            return new QuickAction("Background subtree",
                () => context.PerformRefactoring("Toggle: Background subtree",
                    new ToggleModifierRefactoring(graph, node, ScadNodeModifier.Background, !hasBackground)),
                true, hasBackground);
        }
    }
}