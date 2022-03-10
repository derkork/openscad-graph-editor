using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Widgets
{
    public abstract class LineEditBase : LineEdit, IScadLiteralWidget
    {
        [Signal]
        public delegate void Changed();

        private bool _hasSelectedAll;

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
            // select everything on focus
            this.Connect("focus_entered")
                .To(this, nameof(SelectOnFocusEntered));
        }

        public override void _GuiInput(InputEvent evt)
        {
            if (!(evt is InputEventMouseButton) || _hasSelectedAll)
            {
                return;
            }
            
            // select all when you click on it. We need to do this because otherwise
            // clicking on it will kill the selection. We only do this once while having
            // focus, so you can select text with a second click.
            SelectAll();
            _hasSelectedAll = true;
        }

        private void SelectOnFocusEntered()
        {
            CallDeferred("select_all");
        }

        protected void DeselectAll()
        {
            _hasSelectedAll = false;
            CallDeferred("deselect");
        }
    }
}