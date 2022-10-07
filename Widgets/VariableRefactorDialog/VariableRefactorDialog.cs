using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

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
        private IconButton.IconButton _templateUpButton;
        private IconButton.IconButton _templateDownButton;
        private readonly List<KeyValueLine> _keyValueLines = new List<KeyValueLine>();
        
        
        

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

            _minEdit.Connect("focus_exited")
                .WithBinds(_minEdit, true)
                .To(this, nameof(OnNumericFieldChanged));
            
            _stepEdit.Connect("focus_exited")
                .WithBinds(_stepEdit, true)
                .To(this, nameof(OnNumericFieldChanged));
            
            _maxEdit.Connect("focus_exited")
                .WithBinds(_stepEdit, true)
                .To(this, nameof(OnNumericFieldChanged));
            
            
            _maxLengthContainer = this.WithName<Control>("MaxLengthContainer");
            _maxLengthEdit = this.WithName<LineEdit>("MaxLengthEdit");
            
            _maxLengthEdit.Connect("focus_exited")
                .WithBinds(_maxLengthEdit, false)
                .To(this, nameof(OnNumericFieldChanged));
            
            _keyValuePairContainer = this.WithName<Control>("KeyValuePairContainer");
            _keyValuePairAddButton = this.WithName<IconButton.IconButton>("KeyValuePairAddButton");
            _keyValuePairAddButton.ButtonPressed += OnAddKeyValuePairPressed;
            
            _keyValuePairGrid = this.WithName<GridContainer>("KeyValuePairGrid");
            _templateValueEdit = this.WithName<LineEdit>("TemplateValueEdit");
            _templateLabelEdit = this.WithName<LineEdit>("TemplateLabelEdit");
            _templateUpButton = this.WithName<IconButton.IconButton>("TemplateUpButton");
            _templateDownButton = this.WithName<IconButton.IconButton>("TemplateDownButton");
            _templateDeleteButton = this.WithName<IconButton.IconButton>("TemplateDeleteButton");

            _templateValueEdit.Visible = false;
            _templateLabelEdit.Visible = false;
            _templateDeleteButton.Visible = false;
            _templateUpButton.Visible = false;
            _templateDownButton.Visible = false;
            
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
                    _constraintTypeOptionButton.AddItem("Fixed values", (int) VariableCustomizerConstraintType.Options);
                    break;
                case PortType.String:
                    _constraintTypeOptionButton.AddItem("Maximum length", (int) VariableCustomizerConstraintType.MaxLength);
                    _constraintTypeOptionButton.AddItem("Fixed values", (int) VariableCustomizerConstraintType.Options);
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

            FixKeyValuePairs();
            
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
                case VariableCustomizerConstraintType.Options:
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
            AddKeyValuePair("0", "");            
        }

        private void AddKeyValuePair(string value, string label)
        {
            var entryUuid = Guid.NewGuid().ToString();
            // while we technically don't need it, we give every new item a unique name using the uuid and a string
            // this makes it easier for the test driver to attach to it.
            var valueEdit = _templateValueEdit.Clone();
            valueEdit.Name = $"value-{entryUuid}";
            var labelEdit = _templateLabelEdit.Clone();
            labelEdit.Name = "label-" + entryUuid;
            
            var deleteButton = _templateDeleteButton.Clone();
            deleteButton.Name = "delete-" + entryUuid;
            var upButton = _templateUpButton.Clone();
            upButton.Name = "up-" + entryUuid;
            var downButton = _templateDownButton.Clone();
            downButton.Name = "down-" + entryUuid;

            valueEdit.Visible = true;
            labelEdit.Visible = true;
            deleteButton.Visible = true;
            upButton.Visible = true;
            downButton.Visible = true;

            valueEdit.Text = value;
            labelEdit.Text = label;

            var line = new KeyValueLine(
                valueEdit,
                labelEdit,
                upButton,
                downButton,
                deleteButton
            );
            _keyValueLines.Add(line);


            if (_typeHintOptionButton.SelectedPortType.IsNumericInCustomizer())
            {
                // add a numeric constraint on the values
                valueEdit.Connect("focus_exited")
                    .WithBinds(valueEdit, false)
                    .To(this, nameof(OnNumericFieldChanged));
            }

            deleteButton.ButtonPressed += () => OnRemoveParameterPressed(line);
            upButton.ButtonPressed += () => OnParameterUpPressed(line);
            downButton.ButtonPressed += () => OnParameterDownPressed(line);

            line.InsertInto(_keyValuePairGrid);

            RepaintKeyValuePairs();
            ValidateAll();
            SetAsMinsize();
            
            // give the new  value edit the focus
            valueEdit.GrabFocus();
        }

        private void FixKeyValuePairs()
        {
            // if the variable type is switched from something numeric to something non-numeric or vice versa
            // we need to remove/add all the numeric constraints from/to the value edits and maybe also need
            // to fix up the values.
            var isNumeric = _typeHintOptionButton.SelectedPortType.IsNumericInCustomizer();
            foreach (var line in _keyValueLines)
            {
                if (isNumeric)
                {
                    // if there is currently no numeric constraint, add one
                    if (!line.ValueEdit.IsConnected("focus_exited", this, nameof(OnNumericFieldChanged)))
                    {
                        // add a numeric constraint on the values
                        line.ValueEdit.Connect("focus_exited")
                            .WithBinds(line.ValueEdit, false)
                            .To(this, nameof(OnNumericFieldChanged));
                    }
                    
                    // if the value is not a number, set it to 0
                    if (!line.ValueEdit.Text.SafeTryParse(out _))
                    {
                        line.ValueEdit.Text = "0";
                    }
                }
                else
                {
                    // if there is currently a numeric constraint, remove it
                    if (line.ValueEdit.IsConnected("focus_exited", this, nameof(OnNumericFieldChanged)))
                    {
                        line.ValueEdit.Disconnect("focus_exited", this, nameof(OnNumericFieldChanged));
                    }
                }
            }
        }
        
        private void RepaintKeyValuePairs()
        {
            // remove all keyValuePairs
            _keyValueLines.ForAll(it => it.RemoveFrom(_keyValuePairGrid));
            // and re-insert in correct order
            _keyValueLines.ForAll(it => it.InsertInto(_keyValuePairGrid));
        }

        private void OnRemoveParameterPressed(KeyValueLine line)
        {
            _keyValueLines.Remove(line);
            line.Discard();

            RepaintKeyValuePairs();
        }

        private void OnParameterUpPressed(KeyValueLine line)
        {
            var index = _keyValueLines.IndexOf(line);
            if (index <= 0)
            {
                return; // nothing to do
            }

            // swap the line with the one above. 
            var targetIndex = index - 1;
            var toMoveDown = _keyValueLines[targetIndex];
            _keyValueLines[targetIndex] = line;
            _keyValueLines[index] = toMoveDown;

            RepaintKeyValuePairs();
        }

        private void OnParameterDownPressed(KeyValueLine line)
        {
            var index = _keyValueLines.IndexOf(line);
            if (index == -1 || index + 1 >= _keyValueLines.Count)
            {
                return; // nothing to do
            }

            // swap the line with the one above. 
            var targetIndex = index + 1;
            var toMoveUp = _keyValueLines[targetIndex];
            _keyValueLines[targetIndex] = line;
            _keyValueLines[index] = toMoveUp;

            RepaintKeyValuePairs();
        }

        
        private void OnNumericFieldChanged(LineEdit lineEdit, bool allowEmpty)
        {
            if (allowEmpty && lineEdit.Text == "")
            {
                return;
            }
            
            if (lineEdit.Text.SafeTryParse(out var result))
            {
                // text is numeric, allow it.
                lineEdit.Text = result.SafeToString();
            }
            else
            {
                // revert to a good default value
                lineEdit.Text = allowEmpty ? "" : "0";
            }
        }
        
        
        private class KeyValueLine
        {
            public LineEdit ValueEdit { get; }
            private readonly LineEdit _labelEdit;
            private readonly IconButton.IconButton _upButton;
            private readonly IconButton.IconButton _downButton;
            private readonly IconButton.IconButton _deleteButton;

            public KeyValueLine(LineEdit valueEdit, LineEdit labelEdit, IconButton.IconButton upButton, IconButton.IconButton downButton,
                IconButton.IconButton deleteButton)
            {
                ValueEdit = valueEdit;
                _labelEdit = labelEdit;
                _upButton = upButton;
                _downButton = downButton;
                _deleteButton = deleteButton;
            }

            public void RemoveFrom(Control container)
            {
                container.RemoveChild(ValueEdit);
                container.RemoveChild(_labelEdit);
                container.RemoveChild(_upButton);
                container.RemoveChild(_downButton);
                container.RemoveChild(_deleteButton);
            }

            public void InsertInto(Control container)
            {
                container.AddChild(ValueEdit);
                container.AddChild(_labelEdit);
                container.AddChild(_upButton);
                container.AddChild(_downButton);
                container.AddChild(_deleteButton);
            }

            public void Discard()
            {
                ValueEdit.RemoveAndFree();
                _labelEdit.RemoveAndFree();
                _upButton.RemoveAndFree();
                _downButton.RemoveAndFree();
                _deleteButton.RemoveAndFree();
            }
        }
    }
}