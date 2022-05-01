using System;
using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Widgets.ColorButton
{
    [Tool]
    public class ColorButton : Node
    {
        private Color _color = new Color(1,1,1,1);
        private ColorRect _colorRect;

        public event Action<ColorButton> Pressed;

        [Export]
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                if (_colorRect != null)
                {
                    _colorRect.Color = _color;
                }
            }
        }

        public override void _Ready()
        {
            _colorRect = this.WithName<ColorRect>("ColorRect");
            _colorRect.Color = _color;
            this.WithName<Button>("Button").Connect("pressed")
                .To(this, nameof(OnButtonPressed));
        }

        private void OnButtonPressed()
        {
            Pressed?.Invoke(this);
        }
    }
}