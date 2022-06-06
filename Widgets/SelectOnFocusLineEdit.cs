using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// A line edit that selects all text when focused
    /// </summary>
    public class SelectOnFocusLineEdit : LineEdit
    {
        
        private bool _hasSelectedAll;

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
        
    }
}