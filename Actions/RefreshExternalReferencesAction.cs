using JetBrains.Annotations;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    [UsedImplicitly]
    public class RefreshExternalReferencesAction : IEditorAction
    {
        public int Order => 9000;
        public string Group => "";
        public bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result)
        {
            if (item.IsDirectExternalReference(out var externalReference))
            {
                var title = "Refresh external references";
                result = new QuickAction(title,
                    () => context.PerformRefactoring( title, new RefreshExternalReferencesRefactoring()));
                return true;
            }
            
            result = default;
            return false;
        }
    }
}