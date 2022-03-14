using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Widgets.ScadNodeList
{
    public class ScadNodeList : ItemList
    {
        private List<ScadNodeListEntry> _entries;

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
    }
}