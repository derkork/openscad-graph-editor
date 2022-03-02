using Godot;
using GodotExt;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class Vector3Edit : VBoxContainer, IScadLiteralWidget
    {
        private NumberEdit _x;
        private NumberEdit _y;
        private NumberEdit _z;

        [Signal]
        public delegate void Changed();

        public string RenderedValue => $"[{_x.RenderedValue}, {_y.RenderedValue}, {_z.RenderedValue}]";

        public string SerializedValue
        {
            get => $"{_x.RenderedValue}|{_y.RenderedValue}|{_z.RenderedValue}";
            set
            {
                var parts = value.Split('|');
                _x.SerializedValue = parts[0];
                _y.SerializedValue = parts[1];
                _z.SerializedValue = parts[2];
            }
        }

        public void SetEnabled(bool enabled)
        {
            _x.SetEnabled(enabled);
            _y.SetEnabled(enabled);
            _z.SetEnabled(enabled);
        }

        public void NotifyChanged()
        {
            EmitSignal(nameof(Changed));
        }

        public override void _Ready()
        {
            _x = Prefabs.New<NumberEdit>();
            _x.ConnectChanged().To(this, nameof(NotifyChanged));
            _y = Prefabs.New<NumberEdit>();
            _y.ConnectChanged().To(this, nameof(NotifyChanged));
            _z = Prefabs.New<NumberEdit>();
            _z.ConnectChanged().To(this, nameof(NotifyChanged));

            var xLabel = new Label();
            xLabel.Text = "X";
            xLabel.MoveToNewParent(this);
            _x.MoveToNewParent(this);


            var yLabel = new Label();
            yLabel.Text = "Y";
            yLabel.MoveToNewParent(this);
            _y.MoveToNewParent(this);

            var zLabel = new Label();
            zLabel.Text = "Z";
            zLabel.MoveToNewParent(this);
            _z.MoveToNewParent(this);
        }

        public ConnectExt.ConnectBinding ConnectChanged()
        {
            return this.Connect(nameof(Changed));
        }
    }
}