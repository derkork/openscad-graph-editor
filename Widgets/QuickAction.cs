using System;
using Godot;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// An action that can be loaded into the <see cref="QuickActionsPopup"/>
    /// </summary>
    public class QuickAction
    {
        public string Title { get; }
        public Action OnSelect { get; }
        
        public bool IsCheckbox { get; }
        
        public bool IsChecked { get; }
        
        public QuickAction(string title, Action onSelect, bool isCheckbox = false, bool isChecked = false)
        {
            Title = title;
            OnSelect = onSelect;
            IsCheckbox = isCheckbox;
            IsChecked = isChecked;
        }
        
    }
}