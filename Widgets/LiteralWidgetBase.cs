using System;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public abstract class LiteralWidgetBase<TControl,TLiteral> : HBoxContainer, IScadLiteralWidget where TControl:Control where TLiteral:IScadLiteral
    {
        private Button _disableButton;
        private Button _enableButton;
        public event Action<object> LiteralValueChanged;
        public event Action<bool> LiteralToggled;
        
        protected TControl Control { get; private set; }
        protected TLiteral Literal { get; private set; }

        protected abstract TControl CreateControl();

        public void BindTo(TLiteral literal)
        {
            if (Control == null)
            {
                Control = CreateControl();
                Control.MoveToNewParent(this);
            }

            if (_disableButton == null)
            {
                _disableButton = new Button();
                _disableButton.Text = "X";
                _disableButton.Connect("pressed")
                    .To(this, nameof(OnDisableButtonPressed));
                _disableButton.MoveToNewParent(this);
            }
            
            if (_enableButton == null)
            {
                _enableButton = new Button();
                _enableButton.Text = "Set";
                _enableButton.Connect("pressed")
                    .To(this, nameof(OnEnableButtonPressed));
                _enableButton.MoveToNewParent(this);
            }


            Literal = literal;
            if (literal.IsSet)
            {
                ApplyControlValue();
                Control.Visible = true;
                _disableButton.Visible = true;
                _enableButton.Visible = false;
            }
            else
            {
                Control.Visible = false;
                _disableButton.Visible = false;
                _enableButton.Visible = true;
            }
        }


        protected abstract void ApplyControlValue();
        
        private void OnDisableButtonPressed()
        {
            LiteralToggled?.Invoke( false);
        }
        
        private void OnEnableButtonPressed()
        {
            LiteralToggled?.Invoke( true);
        }


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