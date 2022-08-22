using Godot;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets.StylusDebugDialog
{
    [UsedImplicitly]
    public class StylusDebugDialog : WindowDialog
    {
        private Label _debugLabel;
        
        public override void _Ready()
        {
            _debugLabel= this.WithName<Label>("DebugLabel");
        }

        public override void _GuiInput(InputEvent @event)
        {

            var type = @event.GetType().Name;
            _debugLabel.Text = type;
        }
    }
}
