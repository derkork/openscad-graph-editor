using System;
using Godot;
using GodotTestDriver.Drivers;
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