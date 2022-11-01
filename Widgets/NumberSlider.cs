using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using GodotExt;
using Serilog;


namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class NumberSlider : LiteralWidgetBase<VBoxContainer, NumberLiteral>
    {
        public double Min
        {
            get => _min;
            set
            {
                _min = value;
               SetupSlider();
            }
        }

        public double Step
        {
            get => _step;
            set
            {
                _step = value;
                SetupSlider();
            }
        }

        public double Max
        {
            get => _max;
            set
            {
                _max = value;
                SetupSlider();
            }
        }

        private Label _label;
        private HSlider _slider;
        private double _min;
        private double _step;
        private double _max;

        protected override VBoxContainer CreateControl()
        {
            _label = new Label();
            _slider = new HSlider();

            var result = new VBoxContainer();
            result.SizeFlagsHorizontal = (int) SizeFlags.ExpandFill;
            result.AddChild(_label);

            SetupSlider();
            _slider.Connect("drag_ended")
                .To(this, nameof(OnSliderDragEnded));
            _slider.Connect("value_changed")
                .To(this, nameof(OnSliderValueChanged));

            result.AddChild(_slider);

            return result;
        }

        private void SetupSlider()
        {
            if (_slider == null)
            {
                return;
            }
            _slider.MinValue = Min;
            _slider.MaxValue = Max;
            _slider.Step = Step;
        }

        private void OnSliderDragEnded(bool valueChanged)
        {
            if (valueChanged)
            {
                var number = _slider.Value;
                SetLabelText(number);
                EmitValueChange(new NumberLiteral(number));
            }
        }

        private void OnSliderValueChanged(float value)
        {
            // just update the label while dragging
            SetLabelText(value);
        }

        private void SetLabelText(double number)
        {
            // round the number to 4 digits for the label, but don't add trailing zeros
            _label.Text = number.ToString("0.####");
        }

        protected override void ApplyControlValue()
        {
            // clamp value to min/max
            var clamped = false;
            var clampedValue = Literal.Value;
            if (clampedValue > Max)
            {
                clampedValue = Max;
                clamped = true;
            }
            else if (clampedValue < Min)
            {
                clampedValue = Min;
                clamped = true;
            }

            _slider.Value = clampedValue;
            SetLabelText(clampedValue);

            // if we modified the value, emit a change event
            if (clamped)
            {
                EmitValueChange(new NumberLiteral(clampedValue));
            }
        }
    }
}