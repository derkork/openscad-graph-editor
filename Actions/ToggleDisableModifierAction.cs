using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class ToggleDisableModifierAction : ToggleModifierAction
    {
        public override int Order => 4000;

        protected override QuickAction BuildAction(IEditorContext context, ScadGraph graph, ScadNode node)
        {
           
            var hasDisable = node.GetModifiers().HasFlag(ScadNodeModifier.Disable);
            return    new QuickAction("Disable subtree",
                    () => context.PerformRefactoring("Toggle: Disable subtree",
                        new ToggleModifierRefactoring(graph, node, ScadNodeModifier.Disable, !hasDisable)), true,
                    hasDisable);

        }
    }
}