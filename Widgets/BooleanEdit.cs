using Godot;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets
{
    public class BooleanEdit : CheckBox, IScadLiteralWidget
    {
        [Signal]
        public delegate void Changed();
        
        public string Value
        {
            get => Pressed ? "true" : "false";
            set => Pressed = value == "true";
        }

        public void SetEnabled(bool enabled)
        {
            Disabled = !enabled;
        }

        public ConnectExt.ConnectBinding ConnectChanged()
        {
            return this.Connect(nameof(Changed));
        }

        public override void _Ready()
        {
            this.Connect("toggled")
                .To(this, nameof(NotifyChanged));
        }

        private void NotifyChanged([UsedImplicitly] bool _)
        {
            EmitSignal(nameof(Changed));   
        }
    }
}