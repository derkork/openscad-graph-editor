using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets.ScadItemList
{
    [UsedImplicitly]
    public class ScadItemList : ItemList
    {
        private List<ScadItemListEntry> _entries;
        
        // event called when an item in the list is double-clicked
        public event Action<ScadItemListEntry> ItemActivated;
        
        // event called when an item in the list is right-clicked
        public event Action<ScadItemListEntry, Vector2> ItemContextMenuRequested;
        

        
        public override void _Ready()
        {
            AllowRmbSelect = true;
            this.Connect("item_activated")
                .To(this, nameof(OnItemActivated));
            this.Connect("item_rmb_selected")
                .To(this, nameof(OnItemRmbSelected));
        }

        public void Setup(IEnumerable<ScadItemListEntry> entries)
        {
            Clear();
            _entries = entries.ToList();
            foreach (var entry in _entries)
            {
                AddItem(entry.Title);
            }
        }

        public ScadItemListEntry GetEntry(int index)
        {
            return _entries[index];
        }

        public override object GetDragData(Vector2 position)
        {
            var selectedItems = GetSelectedItems();
            // nothing or more than one item selected, we cannot drag.

            if (selectedItems.Length != 1)
            {
                return null;
            }

            var entry = _entries[selectedItems[0]];
            if (!entry.CanBeDragged)
            {
                return null;
            }

            var panel = new PanelContainer();
            var label = new Label();
            panel.AddChild(label);
            label.Text = entry.Title;
            SetDragPreview(panel);
            return new ScadItemListDragData(this, selectedItems[0]);
        }

        private void OnItemActivated(int index)
        {
            ItemActivated?.Invoke(_entries[index]);
        }

        private void OnItemRmbSelected(int index, [UsedImplicitly] Vector2 _)
        {
            ItemContextMenuRequested?.Invoke(_entries[index], GetGlobalMousePosition());
        }
    }
}