using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
using GodotTestDriver.Util;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Tests.Drivers
{
    /// <summary>
    /// Driver for a popup menu.
    /// </summary>
    [PublicAPI]
    public class PopupMenuDriver<T> : ControlDriver<T> where T:PopupMenu
    {
        public PopupMenuDriver(Func<T> producer, string description = "") : base(producer, description)
        {
        }

        /// <summary>
        /// Returns the amount of items in the popup menu.
        /// </summary>
        public int ItemCount => PresentRoot.GetItemCount();
        
        /// <summary>
        /// Returns the text of the items in the popup menu.
        /// </summary>
        public IEnumerable<string> MenuItems
        {
            get
            {
                for(var i = 0; i < ItemCount; i++)
                {
                    yield return PresentRoot.GetItemText(i);
                }
            }
        }
        
        public bool IsItemChecked(int index)
        {
            return PresentRoot.IsItemChecked(index);
        }
        
        public bool IsItemDisabled(int index)
        {
            return PresentRoot.IsItemDisabled(index);
        }
        
        public bool IsItemSeparator(int index)
        {
            return PresentRoot.IsItemSeparator(index);
        }
        
        public int GetItemId(int index)
        {
            return PresentRoot.GetItemId(index);
        }
        
        /// <summary>
        /// Selects the item at the given index.
        /// </summary>
        public async Task SelectItemAtIndex(int index)
        {
            var popup = VisibleRoot;
            // verify index is in range
            if (index < 0 || index >= ItemCount)
            {
                throw new IndexOutOfRangeException(
                    $"Index {index} is out of range for popup menu with {ItemCount} items.");
            }
            
            // verify that item is not disabled
            if (popup.IsItemDisabled(index))
            {
                throw new InvalidOperationException(
                    $"Item at index {index} is disabled and cannot be selected.");
            }
            
            // verify that item is not a separator
            if (popup.IsItemSeparator(index))
            {
                throw new InvalidOperationException(
                    $"Item at index {index} is a separator and cannot be selected.");
            }
            
            await popup.GetTree().ProcessFrame();
            
            // select item
            // ideally we would use a mouse click here but since the API does not provide the position of
            // each entry, we have to fake it.
            popup.EmitSignal("index_pressed", index);
            popup.Hide();
            await popup.GetTree().WaitForEvents();
        }

        /// <summary>
        /// Selects the item with the given ID.
        /// </summary>
        public async Task SelectItemWithId(int id)
        {
            var popup = PresentRoot;
            for(var i = 0; i < ItemCount; i++)
            {
                if (popup.GetItemId(i) != id)
                {
                    continue;
                }
                await SelectItemAtIndex(i);
                return;
            }
            
            throw new InvalidOperationException(
                $"No item with id {id} found in popup menu.");
        }
        
        /// <summary>
        /// Selects the item with the given text. If multiple items have the same text, the first one is selected.
        /// </summary>
        public async Task SelectItemWithText(string text)
        {
            var popup = PresentRoot;
            for(var i = 0; i < ItemCount; i++)
            {
                if (popup.GetItemText(i) != text)
                {
                    continue;
                }
                await SelectItemAtIndex(i);
                return;
            }
            
            throw new InvalidOperationException(
                $"No item with text {text} found in popup menu.");
        }
    }

    
    /// <summary>
    /// Driver for a popup menu.
    /// </summary>
    [PublicAPI]
    public sealed class PopupMenuDriver : PopupMenuDriver<PopupMenu>
    {
        public PopupMenuDriver(Func<PopupMenu> producer, string description = "") : base(producer, description)
        {
        }
    }
    
}