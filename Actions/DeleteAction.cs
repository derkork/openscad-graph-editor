using JetBrains.Annotations;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class DeleteAction : IEditorAction
    {
        public int Order => 10000;
        public string Group => "";

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.IsEditableInvokable(context, out var invokableDescription))
            {
                var title = "Delete " + invokableDescription.Name;
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title,
                        new DeleteInvokableRefactoring(invokableDescription)));
                return true;
            }

            if (item.IsEditableVariable(context, out var variableDescription))
            {
                var title = "Delete " + variableDescription.Name;
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title,
                        new DeleteVariableRefactoring(variableDescription)));
                return true;
            }

            if (item.IsDirectExternalReference(out var externalReference))
            {
                var title = "Remove reference to " + externalReference.IncludePath;
                result = new QuickAction(title,
                    () => context.PerformRefactoring(title, new DeleteExternalReferenceRefactoring(externalReference)));
                return true;
            }

            result = default;
            return false;
        }
    }
}