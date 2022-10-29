using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class NumberSpinEdit : NumericRangeLiteralWidget<SpinBox,NumberLiteral>
    {
        private SpinBox _spinBox;

        protected override SpinBox CreateControl()
        {
            _spinBox = new SpinBox();

            SetupRange();

            _spinBox.GetLineEdit().Connect("focus_exited")
                .To(this, nameof(OnFocusExited));

            return _spinBox;
        }

        protected override void SetupRange()
        {
            ApplyRange(_spinBox);
        }


        private void OnFocusExited()
        {
            // no != for double
            if (_spinBox.Value > Literal.Value || _spinBox.Value < Literal.Value)
            {
                // only emit if the value has actually changed  
                EmitValueChange(new NumberLiteral(_spinBox.Value));
            }
        }
        

        protected override void ApplyControlValue()
        {
            // if we modified the value, emit a change event
            if (ApplyValue(_spinBox, Literal.Value, out var clampedValue))
            {
                EmitValueChange(new NumberLiteral(clampedValue));
            }
        }
    }
}