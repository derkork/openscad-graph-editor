using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenScadGraphEditor.Utils;
using GodotExt;

namespace OpenScadGraphEditor.Widgets.ScadNodeList
{
    public class ScadItemList : ItemList
    {
        private List<ScadNodeListEntry> _entries;


        public override void _Ready()
        {
            this.Connect("item_activated")
                .To(this, nameof(OnItemActivated));
        }

        public void Setup(IEnumerable<ScadNodeListEntry> entries)
        {
            Clear();
            _entries = entries.ToList();
            foreach (var entry in _entries)
            {
                AddItem(entry.Title);
            }
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
            if (entry.DragActions.Length <= 0)
            {
                // nothing to drag in there
                return null;
            } 
            
            var label = new Label();
            label.Text = entry.Title;
            SetDragPreview(label);
            return entry.DragActions.HoldMyBeer();
        }

        private void OnItemActivated(int index)
        {
            _entries[index].WhenItemActivated();
        }
    }
}