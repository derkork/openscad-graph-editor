using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public abstract class SpinEditLiteralWidgetBase<TControl, TLiteral> : LiteralWidgetBase<TControl, TLiteral>
        where TControl : Control where TLiteral : IScadLiteral
    {
        private double? _min;
        private double _step = 1;
        private double? _max;

        public double? Min
        {
            get => _min;
            set
            {
                _min = value;
                SetupRange();
            }
        }

        public double Step
        {
            get => _step;
            set
            {
                _step = value;
                SetupRange();
            }
        }

        public double? Max
        {
            get => _max;
            set
            {
                _max = value;
                SetupRange();
            }
        }

        protected abstract void SetupRange();

        /// <summary>
        /// Helper method for applying the current min/max/step values to a SpinBox.
        /// </summary>
        protected void ApplyRange([CanBeNull] SpinBox spinBox)
        {
            if (spinBox == null)
            {
                return;
            }

            spinBox.MinValue = Min ?? 0;
            spinBox.AllowLesser = Min == null;
            spinBox.MaxValue = Max ?? 0;
            spinBox.AllowGreater = Max == null;
            spinBox.Step = Step;
        }

        /// <summary>
        /// Helper method for applying a value to a SpinBox and clamp it to the current min/max values if necessary.
        /// </summary>
        protected bool ApplyValue(SpinBox spinBox, double value, out double clampedValue)
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
                spinBox.Value = value;
            }

            clampedValue = value;
            return clamped;
        }
    }
}