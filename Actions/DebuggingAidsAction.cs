using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    public abstract class DebuggingAidsAction :  IEditorAction
    {
        public abstract int Order { get; }
        public string Group => "-Debugging Aids";
        public abstract bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result);
    }
}