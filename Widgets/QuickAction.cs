using System;
using System.Collections.Generic;

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

        public bool IsSubGroup { get; }
        
        public bool IsSeparator { get; }

        public List<QuickAction> Children { get; }

        public QuickAction(string title, Action onSelect, bool isCheckbox = false, bool isChecked = false)
        {
            Title = title;
            OnSelect = onSelect;
            IsCheckbox = isCheckbox;
            IsChecked = isChecked;
            Children = new List<QuickAction>();
        }

        public QuickAction(string title, List<QuickAction> children)
        {
            Children = children;
            Title = title;
            OnSelect = () => { };
            IsSubGroup = true;
        }

        public QuickAction(string title)
        {
            Children = new List<QuickAction>();
            Title = title;
            OnSelect = () => { };
            IsSeparator = true;
        }

    }
}