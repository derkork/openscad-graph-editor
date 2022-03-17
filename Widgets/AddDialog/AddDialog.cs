using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    [UsedImplicitly]
    public class AddDialog : WindowDialog
    {
        private LineEdit _lineEdit;
        private ItemList _itemList;
        private List<AddDialogEntry> _supportedNodes;
        private Predicate<ScadNode> _contextFilter = node => true;
        private Action<ScadNode> _callback;

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

            // todo: the list of available stuff is dynamic and we need a function to refresh it.

            var languageLevelNodes = 
                BuiltIns.LanguageLevelNodes.Select(it => new AddDialogEntry(() => NodeFactory.Build(it)));
            var libraryModules =
                BuiltIns.Modules.Select(it => new AddDialogEntry(() => NodeFactory.Build<ModuleInvocation>(it)));
            var libraryFunctions =
                BuiltIns.Functions.Select(it => new AddDialogEntry(() => NodeFactory.Build<FunctionInvocation>(it)));


            _supportedNodes = languageLevelNodes
                .Union(libraryModules)
                .Union(libraryFunctions)
                .ToList();
        }


        private void Refresh()
        {
            var searchTerm = _lineEdit.Text;


            _itemList.Clear();
            foreach (var entry in _supportedNodes.Where(it =>
                         (it.ExampleNode.NodeTitle.ContainsIgnoreCase(searchTerm) ||
                          it.ExampleNode.NodeDescription.ContainsIgnoreCase(searchTerm)) &&
                         _contextFilter(it.ExampleNode)
                     ))
            {
                _itemList.AddItem(entry.ExampleNode.NodeTitle);
                _itemList.SetItemMetadata(_itemList.GetItemCount() - 1, _supportedNodes.IndexOf(entry));
            }

            if (_itemList.GetItemCount() > 0)
            {
                _itemList.Select(0);
            }
        }


        public void Open(Action<ScadNode> callback, Predicate<ScadNode> contextFilter = null)
        {
            _callback = callback;
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

            var entry = _supportedNodes[(int) _itemList.GetItemMetadata(selectedItems[0])];
            _callback(entry.CreateCopy());
            Visible = false;
        }

        private void OnItemActivated(int index)
        {
            var entry = _supportedNodes[(int) _itemList.GetItemMetadata(index)];
            _callback(entry.CreateCopy());
            Visible = false;
        }
    }
}