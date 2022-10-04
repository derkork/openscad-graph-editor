using System;
using System.Collections.Generic;
using System.Linq;
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
        private LineEdit _descriptionEdit;
        private PortTypeSelector _typeHintOptionButton;

        private VariableDescription _baseDescription;

        private DialogMode _mode = DialogMode.Edit;
        private Button _okButton;
        private Label _errorLabel;
        private ScadProject _currentProject;

        public override void _Ready()
        {
            GetCloseButton().Visible = false;

            _nameEdit = this.WithName<LineEdit>("NameEdit");
            _nameEdit
                .Connect("text_entered")
                .To(this, nameof(OnNameEntered));


            _descriptionEdit = this.WithName<LineEdit>("DescriptionEdit");
            _typeHintOptionButton = this.WithName<PortTypeSelector>("TypeHintOptionButton");

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


        public void OpenForNewVariable(ScadProject currentProject)
        {
            Clear();
            _mode = DialogMode.Create;
            _currentProject = currentProject;
            ValidateAll();
            PopupCentered();
            SetAsMinsize();
        }

        public void Open(VariableDescription description, ScadProject currentProject)
        {
            Clear();
            _mode = DialogMode.Edit;
            _baseDescription = description;
            _currentProject = currentProject;

            _nameEdit.Text = description.Name;
            ValidateAll();
            PopupCentered();
            SetAsMinsize();
        }

        private void Clear()
        {
            _nameEdit.Text = "";
            _baseDescription = null;
            _currentProject = null;
        }

        private void OnOkPressed()
        {
            switch (_mode)
            {
                case DialogMode.Edit:
                    var refactorings = new List<Refactoring>();
                    // rename variable if necessary.
                    if (_baseDescription.Name != _nameEdit.Text)
                    {
                        refactorings.Add(new RenameVariableRefactoring(_baseDescription, _nameEdit.Text));
                    }

                    // if the description text has changed, update it.
                    if (_baseDescription.Description != _descriptionEdit.Text)
                    {
                        refactorings.Add(
                            new ChangeVariableDocumentationRefactoring(_baseDescription, _descriptionEdit.Text));
                    }
                    
                    // if the type hint has changed, update it.
                    if (_baseDescription.TypeHint != _typeHintOptionButton.SelectedPortType)
                    {
                        refactorings.Add(
                            new ChangeVariableTypeRefactoring(_baseDescription, _typeHintOptionButton.SelectedPortType));
                    }
                    
                    RefactoringsRequested?.Invoke(refactorings.ToArray());

                    break;
                case DialogMode.Create:
                    RefactoringsRequested?.Invoke(new Refactoring[]
                    {
                        new IntroduceVariableRefactoring(VariableBuilder
                            .NewVariable(_nameEdit.Text)
                            .WithDescription(_descriptionEdit.Text)
                            .WithType(_typeHintOptionButton.SelectedPortType)
                            .Build()
                        )
                    });
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
                .Check(
                    // we create a variable, then the name must be different from all other variables in the project
                    (_mode == DialogMode.Create &&
                     _currentProject.Variables.Select(it => it.Name).All(it => it != _nameEdit.Text))
                    // we edit a variable, then the name must be different from all other variables in the project 
                    || (_mode == DialogMode.Edit && _currentProject.Variables.Where(it => it != _baseDescription)
                        .Select(it => it.Name).All(it => it != _nameEdit.Text))
                    , "The name is already used in this project."
                )
                .Check(_nameEdit.Text.IsValidIdentifier(),
                    "Name must not be blank and must be only letters, numbers, and underscores and must not start with a number."
                )
                .UpdateUserInterface();
        }

        private void OnNameEntered([UsedImplicitly] string newText)
        {
            if (!_okButton.Disabled)
            {
                OnOkPressed();
            }
        }
    }
}