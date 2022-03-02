using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    [UsedImplicitly]
    public class AddDialog : WindowDialog
    {
        [Signal]
        public delegate void NodeSelected(ScadNode node);

        private LineEdit _lineEdit;
        private ItemList _itemList;
        private List<ScadNode> _supportedNodes;
        private Predicate<ScadNode> _contextFilter = node => true;

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

            _supportedNodes = NodeFactory.GetAllNodes();
        }


        private void Refresh()
        {
            var searchTerm = _lineEdit.Text;


            _itemList.Clear();
            foreach (var node in _supportedNodes.Where(it => (it.NodeTitle.ContainsIgnoreCase(searchTerm) ||
                                                                it.NodeDescription.ContainsIgnoreCase(searchTerm)) &&
                                                               _contextFilter(it)
                                                               ))
            {
                _itemList.AddItem(node.NodeTitle);
                _itemList.SetItemMetadata(_itemList.GetItemCount() - 1, node);
            }

            if (_itemList.GetItemCount() > 0)
            {
                _itemList.Select(0);
            }
        }


        public void Open(Predicate<ScadNode> contextFilter = null)
        {
            _contextFilter = contextFilter ?? (node => true);
            _lineEdit.Text = "";
            Refresh();
            Show();

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

            var node = _itemList.GetItemMetadata(selectedItems[0]) as ScadNode;
            EmitSignal(nameof(NodeSelected), node.Clone());
            Visible = false;
        }

        private void OnItemActivated(int index)
        {
            var node = _itemList.GetItemMetadata(index) as ScadNode;
            EmitSignal(nameof(NodeSelected), NodeFactory.MakeOne(node));
            Visible = false;
        }
    }
}