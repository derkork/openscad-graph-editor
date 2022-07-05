using System;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets.IconButton;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class InvokableRefactorDialogParameterLineDriver : ControlDriver<GridContainer>
    {
        public LineEditDriver NameEdit { get; }
        public OptionButtonDriver Type { get; }
        public IconButtonDriver UpButton { get; }
        public IconButtonDriver DownButton { get; }
        public IconButtonDriver DeleteButton { get; }
        
        public InvokableRefactorDialogParameterLineDriver(string uuid, Func<GridContainer> producer, string description = "") : base(producer, description)
        {
            NameEdit = new LineEditDriver(() => Root?.WithNameOrNull<LineEdit>("name-" + uuid), Description + "-> Name");
            Type = new OptionButtonDriver(() => Root?.WithNameOrNull<OptionButton>("type-" + uuid), Description + "-> Type");
            UpButton = new IconButtonDriver(() => Root?.WithNameOrNull<IconButton>("up-" + uuid), Description + "-> Up");
            DownButton = new IconButtonDriver(() => Root?.WithNameOrNull<IconButton>("down-" + uuid), Description + "-> Down");
            DeleteButton = new IconButtonDriver(() => Root?.WithNameOrNull<IconButton>("delete-" + uuid), Description + "-> Delete");
        }
        
    }
}