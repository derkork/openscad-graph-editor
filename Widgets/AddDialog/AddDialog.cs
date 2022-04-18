using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    [UsedImplicitly]
    public class AddDialog : WindowDialog
    {
        
        private LineEdit _lineEdit;
        private ItemList _itemList;
        private List<IAddDialogEntry> _entries = new List<IAddDialogEntry>();
        private RequestContext _context;
        private Button _filterByContextCheckbox;

        public override void _Ready()
        {
            _lineEdit = this.WithName<LineEdit>("LineEdit");
            _itemList = this.WithName<ItemList>("ItemList");


            _lineEdit.Connect("text_changed")
                .To(this, nameof(OnTextChanged));
            _lineEdit.Connect("text_entered")
                .To(this, nameof(OnTextEntered));
            _itemList.Connect("item_activated")
                .To(this, nameof(OnItemActivated));

            _filterByContextCheckbox = this.WithName<Button>("FilterByContextCheckbox");
            _filterByContextCheckbox
                .Connect("toggled")
                .To(this, nameof(OnFilterByContextCheckboxToggled));
        }
        
        private void Refresh()
        {
            var searchTerm = _lineEdit.Text;

            var filterByContext = _filterByContextCheckbox.Pressed;
            
            _itemList.Clear();
            var entries = _entries.Where(it =>
                (it.Title.ContainsIgnoreCase(searchTerm) ||
                 it.Keywords.ContainsIgnoreCase(searchTerm)) &&
                (!filterByContext || it.Matches(_context))
            );
            
            foreach (var entry in entries)
            {
                _itemList.AddItem(entry.Title);
                _itemList.SetItemIcon(_itemList.GetItemCount() - 1, entry.Icon);
                _itemList.SetItemMetadata(_itemList.GetItemCount() - 1, _entries.IndexOf(entry));
            }

            if (_itemList.GetItemCount() > 0)
            {
                _itemList.Select(0);
            }
        }


        public void Open(List<IAddDialogEntry> entries, RequestContext context)
        {
            _entries = entries;
            _context = context;
            _filterByContextCheckbox.Pressed = true;
            _lineEdit.Text = "";
            Refresh();
            PopupCentered();
            _lineEdit.GrabFocus();
        }

        private void OnTextChanged([UsedImplicitly] string _)
        {
            Refresh();
        }

        private void OnTextEntered([UsedImplicitly] string _)
        {
            var selectedItems = _itemList.GetSelectedItems();
            if (selectedItems.Length != 1)
            {
                return;
            }

            var entry = _entries[(int) _itemList.GetItemMetadata(selectedItems[0])];
            entry.Action(_context);
            Visible = false;
        }

        private void OnItemActivated(int index)
        {
            var entry = _entries[(int) _itemList.GetItemMetadata(index)];
            entry.Action(_context);
            Visible = false;
        }
        
        private void OnFilterByContextCheckboxToggled([UsedImplicitly] bool _)
        {
            _lineEdit.GrabFocus();
            Refresh();
        }
    }
}