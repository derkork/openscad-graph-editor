using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using GodotTestDriver.Input;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.IconButton;
using OpenScadGraphEditor.Widgets.PortContainer;

namespace OpenScadGraphEditor.Tests.Drivers
{
    /// <summary>
    /// Driver for our customized <see cref="ScadNodeWidget"/>
    /// </summary>
    public class ScadNodeWidgetDriver : GraphNodeDriver<ScadNodeWidget>
    {
    
        public ScadNodeWidgetDriver(Func<ScadNodeWidget> producer, string description = "") : base(producer, description)
        {
        }

        /// <summary>
        /// Because we do our custom title rendering we have a separate accessor for the title here.
        /// </summary>
        public string NodeTitle => PresentRoot.GetChildNodes<Label>().First().Text;

        /// <summary>
        /// Deletes the node from the editor.
        /// </summary>
        public async Task Delete()
        {
            await ClickAtSelectionSpot();
            await Viewport.PressKey(KeyList.Delete);
        }
        
        private PortContainer GetPortContainer(Port port)
        {
            var hBoxes = Root?.GetChildNodes<HBoxContainer>()?.ToList();
            if (hBoxes == null)
            {
                return null;
            }

            if (port.PortIndex >= hBoxes.Count)
            {
                return null;
            }
            
            var hBox = hBoxes[port.PortIndex];
            if (port.IsInput)
            {
                return hBox.GetChildNodes<PortContainer>().FirstOrDefault();
            }
            return hBox.GetChildNodes<PortContainer>().LastOrDefault();
        }

        public LabelDriver PortLabel(Port port) =>
            new LabelDriver(() => GetPortContainer(port)?.WithNameOrNull<Label>("Label"),
                $"{Description} -> Port label ({port})");

        public IconButtonDriver ToggleButton(Port port) =>
            new IconButtonDriver(() => GetPortContainer(port)?.WithNameOrNull<IconButton>("IconButton"),
                $"{Description} -> Toggle Button {port}");
        
        public CheckBoxDriver CheckBoxLiteral(Port port) =>
            new CheckBoxDriver(() => GetPortContainer(port)?
                    .WithNameOrNull<Container>("InnerContainer")?
                    .GetChildNodes<HBoxContainer>()?
                    .FirstOrDefault()?
                    .GetChildNodes<CheckBox>()?
                    .FirstOrDefault(),
                $"{Description} -> CheckBox Literal {port}");
        
    }
}