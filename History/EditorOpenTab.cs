using Godot;

namespace OpenScadGraphEditor.History
{
    /// <summary>
    /// Describes a tab that was open in the editor at a certain point in time.
    /// </summary>
    public class EditorOpenTab
    {
        /// <summary>
        /// The ID of the invokable that was loaded in the graph.
        /// </summary>
        public string InvokableId { get; }

        /// <summary>
        /// The scroll offset of the editor in the tab.
        /// </summary>
        public Vector2 ScrollOffset { get; }

        
        public EditorOpenTab(string invokableId, Vector2 scrollOffset)
        {
            InvokableId = invokableId;
            ScrollOffset = scrollOffset;
        }

        public bool DiffersFrom(EditorOpenTab otherOpenTab)
        {
            return InvokableId != otherOpenTab.InvokableId || ScrollOffset != otherOpenTab.ScrollOffset;
        }
    }
}