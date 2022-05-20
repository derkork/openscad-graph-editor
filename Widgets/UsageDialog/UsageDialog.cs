using System;
using System.Collections.Generic;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Widgets.UsageDialog
{
    public class UsageDialog : Control
    {
        public event Action<UsagePointInformation> NodeHighlightRequested;
        
        private ItemList _usageList;
        private Label _title;
        private List<UsagePointInformation> _usages = new List<UsagePointInformation>();
        
        
        public override void _Ready()
        {
            _title = this.WithName<Label>("TitleLabel");
            _usageList = this.WithName<ItemList>("UsageList");
            _usageList
                .Connect("item_activated")
                .To(this, nameof(OnItemActivated));
            
        }


        private void OnItemActivated(int index)
        {
            var usage = _usages[index];
            NodeHighlightRequested?.Invoke(usage);
        }

        public void Open(string title, List<UsagePointInformation> usages)
        {
            _usages = usages;
            _usageList.Clear();
            _title.Text = title;

            foreach (var usage in usages)
            {
                _usageList.AddItem($"{usage.Label}");
            }

            Visible = true;
        }
    }
}