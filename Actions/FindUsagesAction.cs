using JetBrains.Annotations;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class FindUsagesAction : IEditorAction
    {
        public int Order => 1000;
        public string Group => "";
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.IsEditableInvokable(context, out var invokableDescription))
            {
                result = new QuickAction($"Find usages of {invokableDescription.Name}",
                    () => context.FindAndShowUsages(invokableDescription));
                return true;
            }
            
            if (item.IsEditableVariable(context, out var variableDescription))
            {
                result = new QuickAction($"Find usages of {variableDescription.Name}",
                    () => context.FindAndShowUsages(variableDescription));
            }
            
            result = default;
            return false;
        }
    }
}