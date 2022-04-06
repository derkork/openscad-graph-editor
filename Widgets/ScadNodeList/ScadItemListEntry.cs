using System;

namespace OpenScadGraphEditor.Widgets.ScadNodeList
{
    public class ScadItemListEntry
    {
        public readonly string Title;
        public readonly Action WhenItemActivated;
        public readonly DragData[] DragActions;

        public ScadItemListEntry(string title, Action whenItemActivated, params DragData[] dragActions)
        {
            WhenItemActivated = whenItemActivated;
            Title = title;
            DragActions = dragActions;
        }
    }
}