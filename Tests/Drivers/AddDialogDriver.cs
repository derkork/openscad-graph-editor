using System;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class AddDialogDriver : ControlDriver<AddDialog>
    {
        public AddDialogDriver(Func<AddDialog> producer, string description = "") : base(producer, description)
        {
            SearchField = new LineEditDriver(() => Root?.WithNameOrNull<LineEdit>("LineEdit"), Description + " -> Search Field");
            ItemList = new ItemListDriver(() => Root?.WithNameOrNull<ItemList>("ItemList"), Description + " -> Item List");
        }

        public LineEditDriver SearchField { get; }
        public ItemListDriver ItemList { get; }
    }
}