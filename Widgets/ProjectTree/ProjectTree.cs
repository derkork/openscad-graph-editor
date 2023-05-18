using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    [UsedImplicitly]
    public class ProjectTree : Tree
    {
        /// <summary>
        /// The mapping of ids to nodes.
        /// </summary>
        private Dictionary<string, ProjectTreeEntry> _entries = new Dictionary<string, ProjectTreeEntry>();
        /// <summary>
        /// The expanded state of the nodes.
        /// </summary>
        private Dictionary<string, bool> _expanded = new Dictionary<string, bool>();

        // event called when an item in the list is double-clicked
        public event Action<ProjectTreeEntry> ItemActivated;
        
        // event called when an item in the tree is right-clicked
        public event Action<ProjectTreeEntry, Vector2> ItemContextMenuRequested;
        

        
        public override void _Ready()
        {
            AllowRmbSelect = true;
            HideRoot = true;
            this.Connect("item_activated")
                .To(this, nameof(OnItemActivated));
            this.Connect("item_rmb_selected")
                .To(this, nameof(OnItemRmbSelected));
            this.Connect("item_collapsed")
                .To(this, nameof(OnItemCollapsed));
        }

        public void Setup(List<ProjectTreeEntry> entries)
        {
            Clear();
            _entries.Clear();

            var root = GetRoot() ?? CreateItem();
            ApplyChildren(root, entries);
            
            // remove all entries from the expansion state that are no longer in the tree
            _expanded.Keys
                .Except(_entries.Keys)
                .ToList() // avoid concurrent modification
                .ForAll(id => _expanded.Remove(id));
        }

        private void ApplyChildren(TreeItem root, List<ProjectTreeEntry> entries)
        {
            // kill all items.
            var item = root.GetChildren();
            while (item != null)
            {
                var newItem = item.GetNext();
                item.Free();
                item = newItem;
            }
            
            // now add the new items.
            foreach (var entry in entries)
            {
                _entries[entry.Id] = entry;
                var newItem = CreateItem(root);
                newItem.DisableFolding = !entry.CanBeCollapsed;
                newItem.SetMetadata(0, entry.Id);
                newItem.SetText(0, entry.Title);
                newItem.SetIcon(0, entry.Icon);
                newItem.SetIconMaxWidth(0, 32);
                if (_expanded.TryGetValue(entry.Id, out var expanded))
                {
                    newItem.Collapsed = !expanded && entry.CanBeCollapsed;
                }
                else
                {
                    newItem.Collapsed = entry.CanBeCollapsed;
                }
                
                // recurse
                ApplyChildren(newItem, entry.Children);
            }
        }

        public ProjectTreeEntry GetEntry(string index)
        {
            return _entries[index];
        }

        [CanBeNull]
        private ProjectTreeEntry GetSelectedEntry()
        {
            var selectedItem = GetSelected();

            if (selectedItem == null)
            {
                return null;
            }


            var id = selectedItem.GetMetadata(0) as string;
            if (id == null)
            {
                return null;
            }
            
            return _entries.TryGetValue(id, out var result) ? result : null;
        }

        private void OnItemCollapsed(TreeItem item)
        {
            if (!(item.GetMetadata(0) is string id))
            {
                return;
            }
            
            _expanded[id] = !item.Collapsed;
        }
        
        public override object GetDragData(Vector2 position)
        {
            var entry = GetSelectedEntry();
            if (entry == null || !entry.TryGetDragData(out var data))
            {
                return null;
            }

            var panel = new PanelContainer();
            var label = new Label();
            panel.AddChild(label);
            label.Text = entry.Title;
            SetDragPreview(panel);
            return new DragData(data);
        }

        private void OnItemActivated()
        {
            
            var selectedEntry = GetSelectedEntry();
            if (selectedEntry == null)
            {
                return;
            } 
            
            ItemActivated?.Invoke(selectedEntry);
        }

        private void OnItemRmbSelected([UsedImplicitly] Vector2 _)
        {
            var selectedEntry = GetSelectedEntry();
            if (selectedEntry == null)
            {
                return;
            }
            
            ItemContextMenuRequested?.Invoke(selectedEntry, GetGlobalMousePosition());
        }
    }
}