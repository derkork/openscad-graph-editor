using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public class BooleanEdit : LiteralWidgetBase<CheckBox, BooleanLiteral>
    {
        protected override CheckBox CreateControl()
        {
            var checkBox = new CheckBox();
            checkBox.Connect("toggled")
                .To(this, nameof(NotifyChanged));
            
            return checkBox;
        }

        protected override void ApplyControlValue()
        {
            Control.Pressed = Literal.Value;
        }

        private void NotifyChanged(bool value)
        {
            EmitValueChange(new BooleanLiteral(value));
        }
    }
}