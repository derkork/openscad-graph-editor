using JetBrains.Annotations;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class RefactorAction : IEditorAction
    {
        public int Order => 7000;
        public string Group => "";

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.IsEditableInvokable(context, out var invokableDescription))
            {
                result = new QuickAction($"Refactor {invokableDescription.Name}",
                    () => context.OpenRefactorDialog(invokableDescription));
                return true;
            }

            if (item.IsEditableVariable(context, out var variableDescription))
            {
                result = new QuickAction($"Refactor {variableDescription.Name}",
                    () => context.OpenRefactorDialog(variableDescription));
                return true;

            }

            if (item.IsDirectExternalReference(out var externalReference))
            {
                result = new QuickAction($"Edit reference to {externalReference.IncludePath}",
                    () => context.OpenRefactorDialog(externalReference));
                return true;
            }
            
            
            result = default;
            return false;
        }
    }
}