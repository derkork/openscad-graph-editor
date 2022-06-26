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

        private DialogMode _mode = DialogMode.Edit;
        private Button _okButton;
        private Label _errorLabel;

        public override void _Ready()
        {
            GetCloseButton().Visible = false;

            _nameEdit = this.WithName<LineEdit>("NameEdit");
            _errorLabel = this.WithName<Label>("ErrorLabel");
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
            SetAsMinsize();
        }

        public void Open(VariableDescription description)
        {
            Clear();
            _mode = DialogMode.Edit;

            _nameEdit.Text = description.Name;
            ValidateAll();
            PopupCentered();
            SetAsMinsize();
        }

        private void Clear()
        {
            _nameEdit.Text = "";
        }

        private void OnOkPressed()
        {
            switch (_mode)
            {
                case DialogMode.Edit:
                    break;
                case DialogMode.Create:
                    RefactoringsRequested?.Invoke(new Refactoring[]
                        {new IntroduceVariableRefactoring(VariableBuilder.NewVariable(_nameEdit.Text))});
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

        private void OnIdentifierChanged([UsedImplicitly] string _)
        {
            ValidateAll();
            SetAsMinsize();
        }

        private void ValidateAll()
        {
            ValidityChecker.For(_errorLabel, _okButton)
                .Check(_nameEdit.Text.IsValidIdentifier(),
                    "Name must not be blank and must be only letters, numbers, and underscores and must not start with a letter."
                )
                .UpdateUserInterface();
        }
    }
}