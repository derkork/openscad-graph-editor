using Godot;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets
{
    public abstract class LineEditBase : LineEdit, IScadLiteralWidget
    {
        [Signal]
        public delegate void Changed();
        
        public abstract string RenderedValue { get; }

        public string SerializedValue { get => Text; set => Text = value; }

        public ConnectExt.ConnectBinding ConnectChanged()
        {
            return this.Connect(nameof(Changed));
        }

        public void SetEnabled(bool enabled)
        {
            Editable = enabled;
        }

        public override void _Ready()
        {
            this.Connect("text_changed")
                .To(this, nameof(NotifyChanged));
            // select everything on focus
            this.Connect("focus_entered")
                .To(this, nameof(SelectOnFocusEntered));
            // deselect when losing focus
            this.Connect("focus_exited")
                .To(this, nameof(DeselectOnFocusExited));
        }

        private void NotifyChanged([UsedImplicitly] string _)
        {
            EmitSignal(nameof(Changed));
        }

        private void SelectOnFocusEntered()
        {
            CallDeferred("select_all");
        }

        private void DeselectOnFocusExited()
        {
            CallDeferred("deselect");
        }
    }
}