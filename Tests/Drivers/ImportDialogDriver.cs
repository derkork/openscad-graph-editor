using System;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets.ImportDialog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class ImportDialogDriver : ControlDriver<ImportDialog>
    {
        public OptionButtonDriver Mode { get; }
        public OptionButtonDriver PathMode { get; }
        
        public ButtonDriver FileSelectButton { get; }
        
        public FileDialogDriver ImportFileDialog { get; }
        
        public ButtonDriver OkButton { get; }
        public ButtonDriver CancelButton { get; }

        public ImportDialogDriver(Func<ImportDialog> producer, string description = "") : base(producer, description)
        {
            Mode = new OptionButtonDriver(() => Root?.WithNameOrNull<OptionButton>("ImportModeOptionButton"), $"{Description}-> Mode");
            PathMode = new OptionButtonDriver(() => Root?.WithNameOrNull<OptionButton>("PathModeOptionButton"), $"{Description}-> PathMode");
            FileSelectButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("SelectButton"), $"{Description}-> FileSelectButton");
            ImportFileDialog = new FileDialogDriver(() => Root?.WithNameOrNull<FileDialog>("_FileDialog"), $"{Description}-> ImportFileDialog");
            OkButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("OkButton"), $"{Description}-> OkButton");
            CancelButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("CancelButton"), $"{Description}-> CancelButton");
            
        }
    }
}