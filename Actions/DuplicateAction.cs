using JetBrains.Annotations;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class DuplicateAction : IEditorAction
    {
        public int Order => 9000;
        public string Group => "";

        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.IsEditableInvokable(context, out var invokableDescription))
            {
                result = new QuickAction($"Duplicate {invokableDescription.Name}",
                    () => context.PerformRefactoring($"Duplicate {invokableDescription.Name}",
                        new DuplicateInvokableRefactoring(invokableDescription)));
                return true;
            }

            if (item.IsEditableVariable(context, out var variableDescription))
            {
                result = new QuickAction($"Duplicate {variableDescription.Name}",
                    () => context.PerformRefactoring($"Duplicate {variableDescription.Name}",
                        new DuplicateVariableRefactoring(variableDescription)));
                return true;
            }
            
            result = default;
            return false;
        }
    }
}