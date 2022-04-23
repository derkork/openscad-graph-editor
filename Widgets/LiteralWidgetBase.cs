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

        public void BindTo(TLiteral literal, bool isOutput)
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
            _toggleButton.Pressed = literal.IsSet;
            Control.Visible = literal.IsSet;
            
            if (literal.IsSet)
            {
                ApplyControlValue();
            }
        }


        protected abstract void ApplyControlValue();
        

        protected void EmitValueChange(object value)
        {
            LiteralValueChanged?.Invoke(value);
        }

        public void SetEnabled(bool enabled)
        {
            if (Literal.IsSet)
            {
                DoSetEnabled(enabled);
            }
        }
        
        protected abstract void DoSetEnabled(bool enabled);
    }
}