using System;

namespace OpenScadGraphEditor.Widgets.ScadNodeList
{
    public class ScadNodeListEntry
    {
        public readonly string Title;
        public readonly Action WhenItemActivated;
        public readonly DragData[] DragActions;

        public ScadNodeListEntry(string title, Action whenItemActivated, params DragData[] dragActions)
        {
            WhenItemActivated = whenItemActivated;
            Title = title;
            DragActions = dragActions;
        }
    }
}