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
        public event Action<object> LiteralValueChanged;
        public event Action<bool> LiteralToggled;
        
        protected TControl Control { get; private set; }
        protected TLiteral Literal { get; private set; }

        protected abstract TControl CreateControl();

        public void BindTo(TLiteral literal, bool isOutput, bool isAutoSet, bool isConnected)
        {
            void MakeControl()
            {
                if (Control == null)
                {
                    Control = CreateControl();
                    Control.MoveToNewParent(this);
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
                    _toggleButton.ButtonToggled += (value) => LiteralToggled?.Invoke(value);
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

            // Toggle button is only visible for input literals that are unconnected and not auto-set
            _toggleButton.Visible = !isConnected && !isAutoSet && !isOutput;
            _toggleButton.Pressed = literal.IsSet;
            // Control is always visible for output literals, for input literals it is only visible if the literal
            // is unconnected and either auto-set or explicitly enabled.
            Control.Visible = isOutput || (!isConnected && (isAutoSet || literal.IsSet));

            
            ApplyControlValue();
        }


        protected abstract void ApplyControlValue();
        

        protected void EmitValueChange(object value)
        {
            LiteralValueChanged?.Invoke(value);
        }
    }
}