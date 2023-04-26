using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class GoToDefinitionAction : IEditorAction
    {
        public int Order => 1500;
        public string Group => "";

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.IsEditableInvokable(context, out var invokableDescription) && !item.IsEntryPoint())
            {
                result = new QuickAction($"Go to definition of {invokableDescription.Name}",
                    () => context.OpenGraph(invokableDescription)
                );
                return true;
            }

            result = default;
            return false;
        }
    }
}