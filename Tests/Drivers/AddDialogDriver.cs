using System;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class AddDialogDriver : ControlDriver<AddDialog>
    {
        public AddDialogDriver(Func<AddDialog> producer) : base(producer)
        {
            SearchField = new LineEditDriver(() => Root?.WithName<LineEdit>("LineEdit"));
            ItemList = new ItemListDriver(() => Root?.WithName<ItemList>("ItemList"));
        }

        public LineEditDriver SearchField { get; }
        public ItemListDriver ItemList { get; }
    }
}