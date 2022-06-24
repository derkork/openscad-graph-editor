using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets.DocumentationDialog;
using OpenScadGraphEditor.Widgets.HelpDialog;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class HelpDialogDriver : ControlDriver<HelpDialog>
    {
        
        public LabelDriver TitleLabel { get; }
        public LabelDriver DescriptionLabel { get; }
        public ButtonDriver CloseButton { get; }
        
        
        public HelpDialogDriver(Func<HelpDialog> producer, string description = "") : base(producer, description)
        {
            TitleLabel = new LabelDriver(() => Root?.WithNameOrNull<Label>("TitleLabel"), Description + "-> TitleLabel");
            DescriptionLabel = new LabelDriver(() => Root?.WithNameOrNull<Label>("DescriptionLabel"), Description + "-> DescriptionLabel");
            CloseButton = new ButtonDriver(() => Root?.WithNameOrNull<Button>("CloseButton"), Description + "-> CloseButton");
        }

        public LabelDriver GetDescriptionLabel(Port port)
        {
            if (port.IsInput)
            {
                return new LabelDriver(() =>
                    Root?.WithNameOrNull<Control>("LeftContainer")?.GetChildNodes<Label>()?.Skip(port.PortIndex)
                        .FirstOrDefault(), Description + "-> Label " + port);
            }
            else
            {
                return new LabelDriver(() =>
                    Root?.WithNameOrNull<Control>("RightContainer")?.GetChildNodes<Label>()?.Skip(port.PortIndex)
                        .FirstOrDefault(), Description + "-> Label " + port);
            }
        }
        
        
    }
}