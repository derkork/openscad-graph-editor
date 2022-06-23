using System;
using System.Collections.Generic;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets.DocumentationDialog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class DocumentationDialogDriver : ControlDriver<DocumentationDialog>
    {
        
        public TextEditDriver DescriptionEdit { get; }
        public LineEditDriver ReturnValueEdit { get; }
        public ButtonDriver OkButton { get; }
        public ButtonDriver CancelButton { get; }

        public IEnumerable<LineEditDriver> Parameters =>
            BuildDrivers(
                root => root?.WithNameOrNull<Control>("ParametersSection")?.GetChildNodes<LineEdit>(),
                it => new LineEditDriver(it, Description + "-> LineEdit")
            );

        public DocumentationDialogDriver(Func<DocumentationDialog> producer, string description = "") : base(producer, description)
        {
            DescriptionEdit = new TextEditDriver(() => Root?.WithNameOrNull<TextEdit>("DescriptionEdit"), Description + "-> DescriptionEdit");
            ReturnValueEdit = new LineEditDriver(() => Root?.WithNameOrNull<LineEdit>("ReturnValueEdit"), Description + "-> ReturnValueEdit");
            OkButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("OKButton"), Description + "-> OkButton");
            CancelButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("CancelButton"), Description + "-> CancelButton");
            
        }
        
        
    }
}