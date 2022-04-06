using System;
using Godot;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class ScadConfirmationDialog : ConfirmationDialog
    {
        private Action _onConfirm;

        public override void _Ready()
        {
            Connect("confirmed", this, nameof(OnConfirmed));
        }

        public void Open(string message, Action onConfirm)
        {
            DialogText = message;
            _onConfirm = onConfirm;
            PopupCentered();
        }
        
        private void OnConfirmed()
        {
            _onConfirm.Invoke();
        }
        
    }
}