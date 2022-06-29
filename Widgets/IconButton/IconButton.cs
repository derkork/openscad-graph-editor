using System;
using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Widgets.IconButton
{
    [Tool]
    public class IconButton : MarginContainer
    {
        private Button _button;
        private TextureRect _textureRect;
        private Texture _icon;
        private bool _toggleMode;
        private bool _pressed;
        private bool _disabled;
        private string _hintTooltip = "";
        private Vector2 _padding = Vector2.Zero;
        private MarginContainer _marginContainer;

        public event Action<bool> ButtonToggled;
        public event Action ButtonPressed;

        [Export]
        public Texture Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                if (_textureRect != null)
                {
                    _textureRect.Texture = _icon;
                }
            }
        }

        [Export]
        public bool ToggleMode
        {
            get => _toggleMode;
            set
            {
                _toggleMode = value;
                if (_button != null)
                {
                    _button.ToggleMode = value;
                }
            }
        }
        
        [Export]
        public bool Pressed
        {
            get => _pressed;
            set
            {
                _pressed = value;
                if (_button != null)
                {
                    _button.Pressed = value;
                }
            }
        }


        [Export]
        public bool Disabled
        {
            get => _disabled;
            set
            {
                _disabled = value;
                if (_button != null)
                {
                    _button.Disabled = value;
                }

                if (_textureRect != null)
                {
                    var modulate = _textureRect.Modulate;
                    modulate.a = value ? 0.3f : 1f;
                    _textureRect.Modulate = modulate;
                }
            }
        }
        
        [Export]
        public new string HintTooltip
        {
            get => _hintTooltip;
            set
            {
                _hintTooltip = value;
                if (_button != null)
                {
                    _button.HintTooltip = value;
                }
            }
        }

        [Export]
        public Vector2 Padding
        {
            get => _padding;
            set
            {
                _padding = value;
                if (_marginContainer != null)
                {
                    _marginContainer.AddConstantOverride("margin_top", Mathf.RoundToInt(_padding.y));
                    _marginContainer.AddConstantOverride("margin_bottom", Mathf.RoundToInt(_padding.y));
                    _marginContainer.AddConstantOverride("margin_left", Mathf.RoundToInt(_padding.x));
                    _marginContainer.AddConstantOverride("margin_right", Mathf.RoundToInt(_padding.x));
                }
            }
        }
        
        public override void _Ready()
        {
            _button = this.WithName<Button>("Button");
            _button.ToggleMode = _toggleMode;
            _button.Pressed = _pressed;
            _marginContainer = this.WithName<MarginContainer>("MarginContainer");
            _marginContainer.AddConstantOverride("margin_top", Mathf.RoundToInt(_padding.y));
            _marginContainer.AddConstantOverride("margin_bottom", Mathf.RoundToInt(_padding.y));
            _marginContainer.AddConstantOverride("margin_left", Mathf.RoundToInt(_padding.x));
            _marginContainer.AddConstantOverride("margin_right", Mathf.RoundToInt(_padding.x));
            
            _textureRect = this.WithName<TextureRect>("TextureRect");
            _textureRect.Texture = _icon;
            
            _button.Connect("pressed")
                .To( this, nameof(OnButtonPressed));
            _button.Connect("toggled")
                .To( this, nameof(OnButtonToggled));
            
            UpdateSize();
        }

        private void UpdateSize()
        {
            var font = GetFont("font");
            var fontHeight = font.GetHeight();
            if (_textureRect != null)
            {
                _textureRect.RectMinSize = new Vector2(fontHeight, fontHeight);
            }
        }
    
        private void OnButtonPressed()
        {
            ButtonPressed?.Invoke();
        }
    
        private void OnButtonToggled(bool pressed)
        {
            ButtonToggled?.Invoke(pressed);
        }

        public override void _Notification(int what)
        {
            if (what == NotificationThemeChanged)
            {
                UpdateSize();
            }
        }
    }
}