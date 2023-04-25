using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public abstract class ToggleRootModifierAction : ToggleModifierAction
    {
        public override int Order => 2000;

        protected override QuickAction BuildAction(IEditorContext context, ScadGraph graph, ScadNode node)
        {
            var hasRoot = node.GetModifiers().HasFlag(ScadNodeModifier.Root);
            return new QuickAction("Make node root",
                () => context.PerformRefactoring("Toggle: Make node root",
                    new ToggleModifierRefactoring(graph, node, ScadNodeModifier.Root, !hasRoot)), true,
                hasRoot);
        }
    }
}