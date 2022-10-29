using System;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public abstract class LiteralWidgetBase<TControl,TLiteral> : HBoxContainer, IScadLiteralWidget where TControl:Control where TLiteral:IScadLiteral
    {
        private IconButton.IconButton _toggleButton;
        public event Action<IScadLiteral> LiteralValueChanged;
        public event Action<bool> LiteralToggled;
        
        protected TControl Control { get; private set; }
        protected TLiteral Literal { get; private set; }

        protected abstract TControl CreateControl();

        private bool _silenceEvents;

        public void BindTo(TLiteral literal, bool isOutput, bool isAutoSet, bool isConnected)
        {
            // we need this because hte controls may fire events while we update their state programmatically and 
            // we don't want this to happen.
            _silenceEvents = true;
            
            void MakeControl()
            {
                if (Control == null)
                {
                    Control = CreateControl();
                    Control.MoveToNewParent(this);
                    Control.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                }
            }

            void MakeButton()
            {
                if (_toggleButton == null)
                {
                    var centerContainer = new CenterContainer();

                    _toggleButton = Prefabs.InstantiateFromScene<IconButton.IconButton>();
                    _toggleButton.MoveToNewParent(centerContainer);
                    centerContainer.MoveToNewParent(this);

                    _toggleButton.ToggleMode = true;
                    _toggleButton.Icon = Resources.EditIcon;
                    _toggleButton.RectMinSize = new Vector2(20, 20);
                    _toggleButton.ButtonToggled += (value) =>
                    {
                        if (!_silenceEvents)
                        {
                            LiteralToggled?.Invoke(value);
                        }
                    };
                }
            }

            if (isOutput)
            {
                Alignment = AlignMode.End;
                MakeControl();
                MakeButton(); // button goes to the right
            }
            else
            {
                Alignment = AlignMode.Begin;
                MakeButton(); // button goes to the left
                MakeControl();
            }

            Literal = literal;

            // for output literals, the toggle button is visible when port is not set to auto-set
            if (isOutput)
            {
                _toggleButton.Visible = !isAutoSet;
            }
            // for input literals, the toggle button disappears when the port is connected and is only visible when the port is not set to auto-set
            else
            {
                _toggleButton.Visible = !isConnected && !isAutoSet;
            }
            _toggleButton.Pressed = literal.IsSet;
            // the output literal is always visible when the port is auto-set, otherwise it follows the IsSet of the literal
            if (isOutput)
            {
                Control.Visible = isAutoSet || literal.IsSet;
            }
            // for input literals the control is visible when the port is not connected and the port is auto-set otherwise it follows the IsSet of the literal
            else
            {
                Control.Visible = !isConnected && (isAutoSet || literal.IsSet);
            }
            
            ApplyControlValue();

            _silenceEvents = false;
        }


        protected abstract void ApplyControlValue();
        

        protected void EmitValueChange(IScadLiteral value)
        {
            if (!_silenceEvents)
            {
                LiteralValueChanged?.Invoke(value);
            }
        }
    }
}