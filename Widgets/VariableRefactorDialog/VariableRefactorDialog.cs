using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
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
        private OptionButton _constraintTypeOptionButton;
        private CheckBox _showInCustomizerCheckBox;
        private Label _constraintsLabel;
        private Control _constraintSpacer;
        private Control _minStepMaxContainer;
        private LineEdit _minEdit;
        private LineEdit _stepEdit;
        private LineEdit _maxEdit;
        private Control _maxLengthContainer;
        private LineEdit _maxLengthEdit;
        private Control _keyValuePairContainer;
        private IconButton.IconButton _keyValuePairAddButton;
        private GridContainer _keyValuePairGrid;
        private LineEdit _templateValueEdit;
        private LineEdit _templateLabelEdit;
        private IconButton.IconButton _templateDeleteButton;
        

        public override void _Ready()
        {
            GetCloseButton().Visible = false;

            _nameEdit = this.WithName<LineEdit>("NameEdit");
            _nameEdit
                .Connect("text_entered")
                .To(this, nameof(OnNameEntered));


            _descriptionEdit = this.WithName<LineEdit>("DescriptionEdit");
            _typeHintOptionButton = this.WithName<PortTypeSelector>("TypeHintOptionButton");
            _typeHintOptionButton
                .Connect("item_selected")
                .To(this, nameof(OnVariableTypeChanged));

            
            _showInCustomizerCheckBox = this.WithName<CheckBox>("ShowInCustomizerCheckBox");
            _showInCustomizerCheckBox.Connect("toggled")
                .To(this, nameof(OnShowInCustomizerChanged));
            
            _constraintsLabel = this.WithName<Label>("ConstraintsLabel");
            
            _constraintTypeOptionButton = this.WithName<OptionButton>("ConstraintTypeOptionButton");
            _constraintTypeOptionButton
                .Connect("item_selected")
                .To(this, nameof(OnConstraintTypeChanged));

            _constraintSpacer = this.WithName<Control>("ConstraintSpacer");
            
            _minStepMaxContainer = this.WithName<Control>("MinStepMaxContainer");
            _minEdit = this.WithName<LineEdit>("MinEdit");
            _stepEdit = this.WithName<LineEdit>("StepEdit");
            _maxEdit = this.WithName<LineEdit>("MaxEdit");
            
            _maxLengthContainer = this.WithName<Control>("MaxLengthContainer");
            _maxLengthEdit = this.WithName<LineEdit>("MaxLengthEdit");
            
            _keyValuePairContainer = this.WithName<Control>("KeyValuePairContainer");
            _keyValuePairAddButton = this.WithName<IconButton.IconButton>("KeyValuePairAddButton");
            _keyValuePairAddButton.ButtonPressed += OnAddKeyValuePairPressed;
            
            _keyValuePairGrid = this.WithName<GridContainer>("KeyValuePairGrid");
            _templateValueEdit = this.WithName<LineEdit>("TemplateValueEdit");
            _templateLabelEdit = this.WithName<LineEdit>("TemplateLabelEdit");
            _templateDeleteButton = this.WithName<IconButton.IconButton>("TemplateDeleteButton");
            
            
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

            _typeHintOptionButton.Select(0);
            OnVariableTypeChanged(0);

            _showInCustomizerCheckBox.Pressed = false;
            OnShowInCustomizerChanged(false);

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
        
        private void OnShowInCustomizerChanged(bool currentlyDown)
        {
            if (currentlyDown)
            {
                // turn on the customizer parts
                _constraintsLabel.Visible = true;
                _constraintTypeOptionButton.Visible = true;
                // show the relevant parts for the constraint type
                OnConstraintTypeChanged(0); 
            }
            else
            {
                // turn off all customizer parts
                _constraintsLabel.Visible = false;
                _constraintTypeOptionButton.Visible = false;
                _constraintSpacer.Visible = false;
                _minStepMaxContainer.Visible = false;
                _maxLengthContainer.Visible = false;
                _keyValuePairContainer.Visible = false;
            }
            
            SetAsMinsize();
        }
        
        private void OnVariableTypeChanged([UsedImplicitly] int _)
        {
            // save the currently selected constraint type, so we keep it if it is compatible
            var currentConstraintType = (VariableCustomizerConstraintType) _constraintTypeOptionButton.GetSelectedId();
            
            // depending on the variable type we will fill the constraint option button with the appropriate options.
            _constraintTypeOptionButton.Clear();
            _constraintTypeOptionButton.AddItem("None", (int) VariableCustomizerConstraintType.None);
            
            switch (_typeHintOptionButton.SelectedPortType)
            {
                case PortType.Number:
                case PortType.Vector2:
                case PortType.Vector3:
                case PortType.Vector:
                    _constraintTypeOptionButton.AddItem("Min / Step / Max", (int) VariableCustomizerConstraintType.MinStepMax);
                    _constraintTypeOptionButton.AddItem("Fixed values", (int) VariableCustomizerConstraintType.NumericOptions);
                    break;
                case PortType.String:
                    _constraintTypeOptionButton.AddItem("Maximum length", (int) VariableCustomizerConstraintType.MaxLength);
                    _constraintTypeOptionButton.AddItem("Fixed values", (int) VariableCustomizerConstraintType.StringOptions);
                    break;
                // no constraints available, so don't add any options.
            }

            var found = false;
            // if the current constraint type is still available, select it.
            for (var i = 0; i < _constraintTypeOptionButton.GetItemCount(); i++)
            {
                if (_constraintTypeOptionButton.GetItemId(i) == (int) currentConstraintType)
                {
                    _constraintTypeOptionButton.Select(i);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                // if the current constraint type is not available, select the first one.
                _constraintTypeOptionButton.Select(0);
                OnConstraintTypeChanged(0);
            }
            SetAsMinsize();
        }
        
        private void OnConstraintTypeChanged([UsedImplicitly] int _)
        {
            var constraintType = (VariableCustomizerConstraintType) _constraintTypeOptionButton.GetSelectedId();
            switch (constraintType)
            {
                case VariableCustomizerConstraintType.None:
                    // hide all constraint parts
                    _constraintSpacer.Visible = false;
                    _minStepMaxContainer.Visible = false;
                    _maxLengthContainer.Visible = false;
                    _keyValuePairContainer.Visible = false;
                    break;
                case VariableCustomizerConstraintType.MinStepMax:
                    // show the min step max parts + the spacer, hide the rest
                    _constraintSpacer.Visible = true;
                    _minStepMaxContainer.Visible = true;
                    _maxLengthContainer.Visible = false;
                    _keyValuePairContainer.Visible = false;
                    break;
                case VariableCustomizerConstraintType.MaxLength:
                    // show the max length parts + the spacer, hide the rest
                    _constraintSpacer.Visible = true;
                    _minStepMaxContainer.Visible = false;
                    _maxLengthContainer.Visible = true;
                    _keyValuePairContainer.Visible = false; 
                    break;
                case VariableCustomizerConstraintType.NumericOptions:
                case VariableCustomizerConstraintType.StringOptions:
                    // show the key value pair parts + the spacer, hide the rest
                    _constraintSpacer.Visible = true;
                    _minStepMaxContainer.Visible = false;
                    _maxLengthContainer.Visible = false;
                    _keyValuePairContainer.Visible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            SetAsMinsize();
            
        }

        private void OnAddKeyValuePairPressed()
        {
            
        }
    }
}