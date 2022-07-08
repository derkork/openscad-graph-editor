using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
using GodotTestDriver.Input;
using GodotTestDriver.Util;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Tests.Drivers
{
    /// <summary>
    /// A driver for <see cref="Tree"/> controls.
    /// </summary>
    [PublicAPI]
    public class TreeDriver<T> : ControlDriver<T> where T : Tree
    {
        /// <summary>
        /// Returns the currently selected item or null if none is selected.
        /// </summary>
        [CanBeNull]
        public TreeItem SelectedItem => VisibleRoot?.GetSelected();


        /// <summary>
        /// Returns all items in the tree.
        /// </summary>
        public IEnumerable<TreeItem> AllItems
        {
            get
            {
                var root = PresentRoot;
                if (root == null)
                    yield break;
                var item = root.GetRoot();
                while (item != null)
                {
                    yield return item;
                    item = item.GetNext();
                }
            }
        }

        
        public TreeDriver(Func<T> producer, string description = "") : base(producer, description)
        {
        }
        
        /// <summary>
        /// Returns all items which are selectable in any column.
        /// </summary>
        public IEnumerable<TreeItem> GetSelectableItems() => AllItems.Where(it =>  Enumerable.Range(0, PresentRoot.Columns).Any(it.IsSelectable));

        /// <summary>
        /// Selects given item in the tree.
        /// </summary>
        public async Task SelectItem(TreeItem item, int column = -1)
        {
            var root = VisibleRoot;
            root.ScrollToItem(item);
            var center = GetItemCenter(item, column);
            await Viewport.MoveMouseTo(center);
            item.Select(column);

            if (root.SelectMode == Tree.SelectModeEnum.Multi)
            {
                root.EmitSignal("multi_selected", item, column, true);
            }
            else
            {
                root.EmitSignal("item_selected");
            }

            await root.WaitForEvents();
        }

        /// <summary>
        /// Simulates a right mouse click to the item.
        /// </summary>
        public async Task RmbSelectItem(TreeItem item, int column = -1)
        {
            var root = VisibleRoot;
            root.ScrollToItem(item);
            var center = GetItemCenter(item, column);
            await Viewport.MoveMouseTo(center);
            item.Select(column);
            
            root.EmitSignal("item_rmb_selected", center );
            
            await root.WaitForEvents();
        }

        /// <summary>
        /// Activates the given item.
        /// </summary>
        public async Task ActivateItem(TreeItem item)
        {
            var root = VisibleRoot;
            root.ScrollToItem(item);
            var center = GetItemCenter(item, -1);
            await Viewport.MoveMouseTo(center);
            
            // deselect all other items
            foreach (var it in AllItems)
            {
                for (var i = 0; i < root.Columns; i++)
                {
                    it.Deselect(i);
                }   
            }
            
            item.Select(0);
            
            root.EmitSignal("item_activated");

            await root.WaitForEvents();
        }

        private Vector2 GetItemCenter(TreeItem item, int column)
        {
            var itemArea = VisibleRoot.GetItemAreaRect(item, column);
            var center = itemArea.Position + itemArea.Size / 2;
            return center;
        }
    }

    /// <summary>
    /// A driver for <see cref="Tree"/> controls.
    /// </summary>
    [PublicAPI]
    public class TreeDriver : TreeDriver<Tree>
    {
        public TreeDriver(Func<Tree> producer, string description = "") : base(producer, description)
        {
        }
    }
}