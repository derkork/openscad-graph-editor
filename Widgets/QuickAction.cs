using System;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// An action that can be loaded into the <see cref="QuickActionsPopup"/>
    /// </summary>
    public class QuickAction
    {
        public string Title { get; }
        public Action OnSelect { get; }
        
        public QuickAction(string title, Action onSelect)
        {
            Title = title;
            OnSelect = onSelect;
        }
        
    }
}