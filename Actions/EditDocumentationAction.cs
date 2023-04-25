using JetBrains.Annotations;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class EditDocumentationAction : IEditorAction
    {
        public int Order => 8000;
        public string Group => "";
        
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.IsEditableInvokable(context, out var invokableDescription))
            {
                result = new QuickAction($"Edit documentation of {invokableDescription.Name}",
                    () => context.OpenDocumentationDialog(invokableDescription));
                return true;
            }
            
            result = default;
            return false;
        }
    }
}