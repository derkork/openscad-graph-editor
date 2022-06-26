using System;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class InvokableRefactorDialogParameterLineDriver : ControlDriver<GridContainer>
    {
        public LineEditDriver NameEdit { get; }
        public OptionButtonDriver Type { get; }
        public ButtonDriver UpButton { get; }
        public ButtonDriver DownButton { get; }
        public ButtonDriver DeleteButton { get; }
        
        public InvokableRefactorDialogParameterLineDriver(string uuid, Func<GridContainer> producer, string description = "") : base(producer, description)
        {
            NameEdit = new LineEditDriver(() => Root?.WithNameOrNull<LineEdit>("name-" + uuid), Description + "-> Name");
            Type = new OptionButtonDriver(() => Root?.WithNameOrNull<OptionButton>("type-" + uuid), Description + "-> Type");
            UpButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("up-" + uuid), Description + "-> Up");
            DownButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("down-" + uuid), Description + "-> Down");
            DeleteButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("delete-" + uuid), Description + "-> Delete");
        }
        
    }
}