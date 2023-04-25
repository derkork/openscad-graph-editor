using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    /// <summary>
    /// An editor action is an action that applies to something in the editor. It is used to build quick actions
    /// for item context menus. When a context menu for an item is requested, the editor will ask all editor actions
    /// to contribute a quick action. All contributed quick actions will be displayed in the context menu in the order
    /// specified by the <see cref="Order"/> property. The <see cref="Group"/> property can be used to visually group
    /// quick actions into submenus. If the group is empty, the quick action will be displayed directly in the context
    /// menu. If the group starts with a "-" the group will be displayed with a header and a separator before the
    /// group instead of a submenu.
    /// 
    /// Editor actions are automatically discovered by the <see cref="IEditorActionFactory"/> if they have a parameterless
    /// constructor. For other cases, a custom <see cref="IEditorActionFactory"/> can be implemented manually.
    /// </summary>
    public interface IEditorAction
    {
        /// <summary>
        /// The order in which the action should be displayed. Lower values are displayed first.
        /// </summary>
        public int Order { get; }
        /// <summary>
        /// The group in which the action should be displayed. Actions with a group are visually grouped together and
        /// will receive a group header.
        /// </summary>
        public string Group { get; }
        
        /// <summary>
        /// Build a quick action for the given object and editor context. Returns false if no quick action is available.
        /// </summary>
        bool TryBuildQuickAction(IEditorContext context, RequestContext item, out QuickAction result);
    }
}