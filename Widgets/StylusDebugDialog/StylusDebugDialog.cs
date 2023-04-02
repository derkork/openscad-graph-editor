using System;
using Godot;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets.StylusDebugDialog
{
    [UsedImplicitly]
    public class StylusDebugDialog : WindowDialog
    {
        private TextEdit _debugEdit;
        private int _motionEventCount;
        private Vector2 _motionStart;
        private Vector2 _motionEnd;
        
        public override void _Ready()
        {
            _debugEdit = this.WithName<TextEdit>("DebugEdit");
            
            this.WithName<Control>("TestContainer")
                .Connect("gui_input")
                .To(this, nameof(OnGuiInput));

            this.WithName<Button>("CopyToClipboardButton")
                .Connect("pressed")
                .To(this, nameof(OnCopyToClipboardPressed));

            this.WithName<Button>("ClearButton")
                .Connect("pressed")
                .To(this, nameof(OnClearButtonPressed));
        }

        private void OnGuiInput(InputEvent @event)
        {
            // count motion events and compress them into a single output line
            if (@event is InputEventMouseMotion motionEvent)
            {
                if (_motionEventCount == 0)
                {
                    _motionStart = motionEvent.Position;
                    AddToDebugEdit("Mouse Motion begin at: " + _motionStart + "\n");
                }
                _motionEnd = motionEvent.Position;
                _motionEventCount++;
                return;
            }
            
            // if we have pending motion events, output them now
            if (_motionEventCount > 0)
            {
                AddToDebugEdit( $"Mouse Motion ends  ({_motionEventCount} motion events suppressed) at: {_motionEnd}\n");
                _motionEventCount = 0;
            }      
            
            if (@event is InputEventMouseButton buttonEvent)
            {
                AddToDebugEdit($"Button {buttonEvent.ButtonIndex} pressed: {buttonEvent.Pressed}\n");
                return;
            }
            
            if (@event is InputEventKey keyEvent)
            {
                AddToDebugEdit($"Key {keyEvent.Scancode} pressed: {keyEvent.Pressed}\n");
                return;
            }
            
            // other events, just print their type
            AddToDebugEdit("{@event.GetType().Name}\n");
        }
        
        private void AddToDebugEdit(string text)
        {
            _debugEdit.Text += text;
            _debugEdit.ScrollVertical = double.MaxValue;
        }
        
        private void OnCopyToClipboardPressed()
        {
            OS.Clipboard = _debugEdit.Text;
        }
       
        
        private void OnClearButtonPressed()
        {
            _debugEdit.Text = "";
        }
    }
}
