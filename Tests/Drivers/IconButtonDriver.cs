using System;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
using OpenScadGraphEditor.Widgets.IconButton;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class IconButtonDriver : ControlDriver<IconButton>
    {
        public IconButtonDriver(Func<IconButton> producer, string description = "") : base(producer, description)
        {
        }
        
        public bool IsPressed => PresentRoot.Pressed;
        
        public override async Task ClickCenter(ButtonList mouseButton = ButtonList.Left)
        {
            var button = VisibleRoot;
            if (button.Disabled)
            {
                throw new InvalidOperationException("Button is disabled");
            }

            await base.ClickCenter(mouseButton);
        }
    }
}