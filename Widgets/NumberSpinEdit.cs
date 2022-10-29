using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class NumberSpinEdit : LiteralWidgetBase<SpinBox, NumberLiteral>
    {
        public double? Min
        {
            get => _min;
            set
            {
                _min = value;
                SetupBox();
            }
        }

        public double Step
        {
            get => _step;
            set
            {
                _step = value;
                SetupBox();
            }
        }

        public double? Max
        {
            get => _max;
            set {
                _max = value;
                SetupBox();
            }
        }

        private SpinBox _spinBox;
        private double? _min;
        private double _step = 1;
        private double? _max;

        protected override SpinBox CreateControl()
        {
            _spinBox = new SpinBox();

            SetupBox();

            _spinBox.GetLineEdit().Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _spinBox.Connect("value_changed")
                .To(this, nameof(OnValueChanged));

            return _spinBox;
        }

        private void SetupBox()
        {
            if (_spinBox == null)
            {
                return;
            }
            _spinBox.MinValue = Min ?? 0;
            _spinBox.AllowLesser = Min == null;
            _spinBox.MaxValue = Max ?? 0;
            _spinBox.AllowGreater = Max == null;
            _spinBox.Step = Step;
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
        
        private void OnValueChanged(float value)
        {
            var clamped = false;
            // clamp the value if necessary
            if (Min != null && value < Min)
            {
                value = (float) Min;
                clamped = true;
            }
            if (Max != null && value > Max)
            {
                value = (float) Max;
                clamped = true;
            }

            if (clamped)
            {
                // overwrite value
                _spinBox.Value = value;
            }
        }

        protected override void ApplyControlValue()
        {
            // clamp value to min/max
            var clamped = false;
            var clampedValue = Literal.Value;
            if (Max != null && clampedValue > Max)
            {
                clampedValue = (double) Max;
                clamped = true;
            }
            else if (Min != null && clampedValue < Min)
            {
                clampedValue = (double) Min;
                clamped = true;
            }

            _spinBox.Value = clampedValue;

            // if we modified the value, emit a change event
            if (clamped)
            {
                EmitValueChange(new NumberLiteral(clampedValue));
            }

        }
    }
}