using System;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.VariableRefactorDialog
{
    [UsedImplicitly]
    public class VariableRefactorDialog : WindowDialog
    {
        public event Action<Refactoring[]> RefactoringsRequested;

        private LineEdit _nameEdit;
        private VariableDescription _baseDescription;
        
        private DialogMode _mode = DialogMode.Edit;
        private Button _okButton;

        public override void _Ready()
        {
            GetCloseButton().Visible = false;

            _nameEdit = this.WithName<LineEdit>("NameEdit");
            _nameEdit
                .Connect("text_changed")
                .To(this, nameof(OnIdentifierChanged));

            _okButton = this.WithName<Button>("OkButton");
            _okButton
                .Connect("pressed")
                .To(this, nameof(OnOkPressed));

            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelPressed));
        }


        public void OpenForNewVariable()
        {
            Clear();
            _mode = DialogMode.Create;
            ValidateAll();
            PopupCentered();
        }

        public void Open(VariableDescription description)
        {
            Clear();
            _baseDescription = description;
            _mode = DialogMode.Edit;

            _nameEdit.Text = description.Name;
            ValidateAll();
            PopupCentered();
        }

        private void Clear()
        {
            _baseDescription = null;
            _nameEdit.Text = "";
        }

        private void OnOkPressed()
        {
            switch (_mode)
            {
                case DialogMode.Edit:
                    break;
                case DialogMode.Create:
                    RefactoringsRequested?.Invoke(new Refactoring[]{new IntroduceVariableRefactoring(VariableBuilder.NewVariable(_nameEdit.Text))});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Hide();
        }

        private void OnCancelPressed()
        {
            Hide();
        }

        private void OnIdentifierChanged(string _)
        {
            ValidateAll();
        }

        private void ValidateAll()
        {
            // name must be valid
            var isValid = _nameEdit.Text.IsValidIdentifier();
            _okButton.Disabled = !isValid;
        }
    }
}