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

        private ScadProject _currentProject;
        private VariableDescription _baseDescription;
        private DialogMode _mode = DialogMode.Edit;

        private LineEdit _nameEdit;
        private LineEdit _descriptionEdit;
        private PortTypeSelector _typeHintOptionButton;
        private Label _defaultValueLabel;
        private Control _defaultValueControl;
        [CanBeNull] private IScadLiteral _defaultValueLiteral;

        private OptionButton _constraintTypeOptionButton;
        private CheckBox _showInCustomizerCheckBox;
        private Label _customizerTabLabel;
        private LineEdit _customizerTabEdit;
        private Label _constraintsLabel;
        private Control _constraintSpacer;
        private Control _minStepMaxContainer;
        private LineEdit _minStepMaxMinEdit;
        private LineEdit _minStepMaxStepEdit;
        private LineEdit _minStepMaxMaxEdit;
        private Control _maxLengthContainer;
        private Control _maxContainer;
        private LineEdit _maxEdit;

        private Control _stepContainer;
        private LineEdit _stepEdit;
        private LineEdit _maxLengthEdit;
        private Control _keyValuePairContainer;
        private IconButton.IconButton _keyValuePairAddButton;
        private GridContainer _keyValuePairGrid;
        private LineEdit _templateValueEdit;
        private LineEdit _templateLabelEdit;
        private IconButton.IconButton _templateDeleteButton;
        private IconButton.IconButton _templateUpButton;
        private IconButton.IconButton _templateDownButton;
        private readonly List<ValueLabelLine> _valueLabelLines = new List<ValueLabelLine>();

        private Button _okButton;
        private Label _errorLabel;


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

            _defaultValueLabel = this.WithName<Label>("DefaultValueLabel");
            _defaultValueControl = this.WithName<Control>("DefaultValueControl");

            _showInCustomizerCheckBox = this.WithName<CheckBox>("ShowInCustomizerCheckBox");
            _showInCustomizerCheckBox.Connect("toggled")
                .To(this, nameof(OnShowInCustomizerChanged));

            _customizerTabLabel = this.WithName<Label>("CustomizerTabLabel");
            _customizerTabEdit = this.WithName<LineEdit>("CustomizerTabEdit");

            _constraintsLabel = this.WithName<Label>("ConstraintsLabel");

            _constraintTypeOptionButton = this.WithName<OptionButton>("ConstraintTypeOptionButton");
            _constraintTypeOptionButton
                .Connect("item_selected")
                .To(this, nameof(OnConstraintTypeChanged));

            _constraintSpacer = this.WithName<Control>("ConstraintSpacer");

            _minStepMaxContainer = this.WithName<Control>("MinStepMaxContainer");
            _minStepMaxMinEdit = _minStepMaxContainer.WithName<LineEdit>("MinEdit");
            _minStepMaxStepEdit = _minStepMaxContainer.WithName<LineEdit>("StepEdit");
            _minStepMaxMaxEdit = _minStepMaxContainer.WithName<LineEdit>("MaxEdit");

            _minStepMaxMinEdit.Connect("focus_exited")
                .WithBinds(_minStepMaxMinEdit, false, false)
                .To(this, nameof(OnNumericFieldChanged));

            _minStepMaxStepEdit.Connect("focus_exited")
                .WithBinds(_minStepMaxStepEdit, false, true)
                .To(this, nameof(OnNumericFieldChanged));

            _minStepMaxMaxEdit.Connect("focus_exited")
                .WithBinds(_minStepMaxMaxEdit, false, false)
                .To(this, nameof(OnNumericFieldChanged));


            _maxLengthContainer = this.WithName<Control>("MaxLengthContainer");
            _maxLengthEdit = this.WithName<LineEdit>("MaxLengthEdit");
            _maxLengthEdit.Connect("focus_exited")
                .WithBinds(_maxLengthEdit, false, false)
                .To(this, nameof(OnNumericFieldChanged));

            _maxContainer = this.WithName<Control>("MaxContainer");
            _maxEdit = _maxContainer.WithName<LineEdit>("MaxEdit");
            _maxEdit.Connect("focus_exited")
                .WithBinds(_maxEdit, false, false)
                .To(this, nameof(OnNumericFieldChanged));

            _stepContainer = this.WithName<Control>("StepContainer");
            _stepEdit = _stepContainer.WithName<LineEdit>("StepEdit");
            _stepEdit.Connect("focus_exited")
                .WithBinds(_stepEdit, false, false)
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
            _typeHintOptionButton.Select(0);
            OnVariableTypeChanged(0);

            _showInCustomizerCheckBox.Pressed = false;
            OnShowInCustomizerChanged(false);

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
            _descriptionEdit.Text = description.Description;
            _defaultValueLiteral = description.DefaultValue;
            _typeHintOptionButton.SelectedPortType = description.TypeHint;
            // ensure everything is set up for the given type
            OnVariableTypeChanged(0);

            _showInCustomizerCheckBox.Pressed = description.ShowInCustomizer;

            // ensure the customizer option button has all values filled in.
            OnShowInCustomizerChanged(description.ShowInCustomizer);

            if (description.ShowInCustomizer)
            {
                // loop over the existing items and select the one that is in our description
                for (var i = 0; i < _constraintTypeOptionButton.GetItemCount(); i++)
                {
                    if (_constraintTypeOptionButton.GetItemId(i) ==
                        (int) description.CustomizerDescription.ConstraintType)
                    {
                        _constraintTypeOptionButton.Select(i);
                        break;
                    }
                }

                // restore customizer settings
                switch (description.CustomizerDescription.ConstraintType)
                {
                    case VariableCustomizerConstraintType.None:
                        break;
                    case VariableCustomizerConstraintType.MinStepMax:
                        _minStepMaxMinEdit.Text = description.CustomizerDescription.Min;
                        _minStepMaxStepEdit.Text = description.CustomizerDescription.Step;
                        _minStepMaxMaxEdit.Text = description.CustomizerDescription.Max;
                        break;
                    case VariableCustomizerConstraintType.Max:
                        _maxEdit.Text = description.CustomizerDescription.Max;
                        break;
                    case VariableCustomizerConstraintType.Step:
                        _stepEdit.Text = description.CustomizerDescription.Step;
                        break;
                    case VariableCustomizerConstraintType.MaxLength:
                        _maxLengthEdit.Text = description.CustomizerDescription.MaxLength;
                        break;
                    case VariableCustomizerConstraintType.Options:
                        foreach (var item in description.CustomizerDescription.ValueLabelPairs)
                        {
                            switch (item.Value)
                            {
                                // we only have numbers or string literals so we can limit our choices to these two.
                                case StringLiteral stringLiteral:
                                    AddValueLabelPair(stringLiteral.Value, item.Label.Value);
                                    break;
                                case NumberLiteral numberLiteral:
                                    AddValueLabelPair(numberLiteral.Value.SafeToString(),
                                        item.Label.Value);
                                    break;
                            }
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _customizerTabEdit.Text = description.CustomizerDescription.Tab;

                // and then refresh the ui
                OnConstraintTypeChanged(0);
            }

            PopupCentered();
            ValidateAll();
        }

        private void Clear()
        {
            _nameEdit.Text = "";
            _descriptionEdit.Text = "";
            _customizerTabEdit.Text = "";
            _baseDescription = null;
            _currentProject = null;
            _minStepMaxMinEdit.Text = "0";
            _minStepMaxStepEdit.Text = "1";
            _minStepMaxMaxEdit.Text = "1";
            _maxEdit.Text = "0";
            _stepEdit.Text = "1";
            _maxLengthEdit.Text = "100";

            _valueLabelLines.ForAll(it => it.Discard());
            _valueLabelLines.Clear();

            _defaultValueLiteral = null;
        }

        private void OnOkPressed()
        {
            // build new customizer settings
            var customizerDescription = BuildVariableCustomizerDescription();

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
                            new ChangeVariableTypeRefactoring(_baseDescription,
                                _typeHintOptionButton.SelectedPortType));
                    }

                    // add the new customizer settings (since this is very cheap we just do it every time)
                    refactorings.Add(
                        new ChangeVariableCustomizerSettingsRefactoring(
                            _baseDescription, _showInCustomizerCheckBox.Pressed, customizerDescription));

                    // set the new default value (we also do this every time since it is cheap)
                    refactorings.Add(
                        new ChangeVariableDefaultValueRefactoring(_baseDescription, _defaultValueLiteral));

                    RefactoringsRequested?.Invoke(refactorings.ToArray());

                    break;
                case DialogMode.Create:
                    var newVariable = VariableBuilder
                        .NewVariable(_nameEdit.Text)
                        .WithDescription(_descriptionEdit.Text)
                        .WithType(_typeHintOptionButton.SelectedPortType)
                        .Build();
                    newVariable.ShowInCustomizer = _showInCustomizerCheckBox.Pressed;
                    newVariable.CustomizerDescription = customizerDescription;
                    newVariable.DefaultValue = _defaultValueLiteral;

                    RefactoringsRequested?.Invoke(new Refactoring[]
                    {
                        new IntroduceVariableRefactoring(newVariable)
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Hide();
        }

        /// <summary>
        /// Builds a new <see cref="VariableCustomizerDescription"/> based on the current state of the dialog.
        /// </summary>
        private VariableCustomizerDescription BuildVariableCustomizerDescription()
        {
            // if the customizer is not shown, we don't need to build anything, and return the default value.
            if (!_showInCustomizerCheckBox.Pressed)
            {
                return new VariableCustomizerDescription();
            }
            
            var customizerDescription = new VariableCustomizerDescription
            {
                ConstraintType = (VariableCustomizerConstraintType) _constraintTypeOptionButton.GetSelectedId(),
                Tab = _customizerTabEdit.Text
            };
            

            switch (customizerDescription.ConstraintType)
            {
                case VariableCustomizerConstraintType.None:
                    break;
                case VariableCustomizerConstraintType.MinStepMax:
                    customizerDescription.Min = _minStepMaxMinEdit.Text;
                    customizerDescription.Step = _minStepMaxStepEdit.Text;
                    customizerDescription.Max = _minStepMaxMaxEdit.Text;
                    break;
                case VariableCustomizerConstraintType.MaxLength:
                    customizerDescription.MaxLength = _maxLengthEdit.Text;
                    break;
                case VariableCustomizerConstraintType.Max:
                    customizerDescription.Max = _maxEdit.Text;
                    break;
                case VariableCustomizerConstraintType.Step:
                    customizerDescription.Step = _stepEdit.Text;
                    break;
                case VariableCustomizerConstraintType.Options:
                    // convert the options into literals
                    foreach (var valueLabelLine in _valueLabelLines)
                    {
                        if (_typeHintOptionButton.SelectedPortType.IsNumericInCustomizer())
                        {
                            customizerDescription.ValueLabelPairs.Add((
                                new NumberLiteral(valueLabelLine.ValueEdit.Text.SafeParse()),
                                new StringLiteral(valueLabelLine.LabelEdit.Text)));
                        }
                        else
                        {
                            customizerDescription.ValueLabelPairs.Add((new StringLiteral(valueLabelLine.ValueEdit.Text),
                                new StringLiteral(valueLabelLine.LabelEdit.Text)));
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return customizerDescription;
        }

        private void OnCancelPressed()
        {
            Hide();
        }

        private void OnIdentifierChanged([UsedImplicitly] string _)
        {
            ValidateAll();
        }

        private void ValidateAll()
        {
            var usesLabelValuePairsConstraint = _showInCustomizerCheckBox.Pressed
                                                && _constraintTypeOptionButton.GetSelectedId() ==
                                                (int) VariableCustomizerConstraintType.Options;

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
                .Check(
                    // the values in the value label pairs must be unique
                    !usesLabelValuePairsConstraint
                    || _valueLabelLines.Select(it => it.ValueEdit.Text).Distinct().Count() == _valueLabelLines.Count,
                    "Every fixed value must be unique."
                )
                .Check(
                    // the labels in the value label pairs must be unique
                    !usesLabelValuePairsConstraint
                    ||
                    _valueLabelLines
                        // use the value if no label is set
                        .Select(it => it.LabelEdit.Text.Length == 0 ? it.ValueEdit.Text : it.LabelEdit.Text)
                        .Distinct()
                        .Count() == _valueLabelLines.Count,
                    "Every label of the fixed values must be unique."
                )
                .Check(
                    // at least one value label pair must be set
                    !usesLabelValuePairsConstraint
                    || _valueLabelLines.Count > 0,
                    "At least one fixed value must be set."
                )
                .UpdateUserInterface();
            SetAsMinsize();
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
                _customizerTabLabel.Visible = true;
                _customizerTabEdit.Visible = true;
                _constraintsLabel.Visible = true;
                _constraintTypeOptionButton.Visible = true;
                // show the relevant parts for the constraint type
                OnConstraintTypeChanged(0);
            }
            else
            {
                // turn off all customizer parts
                _customizerTabLabel.Visible = false;
                _customizerTabEdit.Visible = false;
                _constraintsLabel.Visible = false;
                _constraintTypeOptionButton.Visible = false;
                _constraintSpacer.Visible = false;
                _minStepMaxContainer.Visible = false;
                _maxLengthContainer.Visible = false;
                _maxContainer.Visible = false;
                _keyValuePairContainer.Visible = false;
            }
            RebuildLiteralWidget();
            ValidateAll();
        }

        private void OnVariableTypeChanged([UsedImplicitly] int _)
        {
            RebuildLiteralWidget();

            // save the currently selected constraint type, so we keep it if it is compatible
            var currentConstraintType = (VariableCustomizerConstraintType) _constraintTypeOptionButton.GetSelectedId();

            // depending on the variable type we will fill the constraint option button with the appropriate options.
            _constraintTypeOptionButton.Clear();
            _constraintTypeOptionButton.AddItem("None", (int) VariableCustomizerConstraintType.None);

            switch (_typeHintOptionButton.SelectedPortType)
            {
                case PortType.Number:
                    _constraintTypeOptionButton.AddItem("Min / Step / Max",
                        (int) VariableCustomizerConstraintType.MinStepMax);
                    _constraintTypeOptionButton.AddItem("Max", (int) VariableCustomizerConstraintType.Max);
                    _constraintTypeOptionButton.AddItem("Step", (int) VariableCustomizerConstraintType.Step);
                    _constraintTypeOptionButton.AddItem("Fixed values", (int) VariableCustomizerConstraintType.Options);
                    break;
                case PortType.Vector2:
                case PortType.Vector3:
                case PortType.Vector:
                    _constraintTypeOptionButton.AddItem("Min / Step / Max",
                        (int) VariableCustomizerConstraintType.MinStepMax);
                    break;
                case PortType.String:
                    _constraintTypeOptionButton.AddItem("Maximum length",
                        (int) VariableCustomizerConstraintType.MaxLength);
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

            ValidateAll();
        }

        private void RebuildLiteralWidget()
        {
            // change the widget to match the type
            var newLiteral = _typeHintOptionButton.SelectedPortType.GetMatchingLiteralType().BuildLiteral();

            // if the type has changed, then use the new type, otherwise keep the existing literal to not overwrite
            // the value with an empty one
            if (newLiteral?.GetType() != _defaultValueLiteral?.GetType())
            {
                _defaultValueLiteral = newLiteral;
            }


            var newLiteralWidget =
                _defaultValueLiteral.BuildWidget(false, true, false, _defaultValueControl as IScadLiteralWidget,
                    BuildVariableCustomizerDescription());

            if (newLiteralWidget != null)
            {
                // check if we re-used the old widget and if not, replace the old widget with the new one
                if (newLiteralWidget != _defaultValueControl)
                {
                    newLiteralWidget.LiteralValueChanged += literal => _defaultValueLiteral = literal;
                    _defaultValueControl.GetParent()
                        .AddChildBelowNode(_defaultValueControl, (Control) newLiteralWidget);
                    _defaultValueControl.RemoveAndFree();
                    _defaultValueControl = (Control) newLiteralWidget;
                }

                // make sure the default value line is visible
                _defaultValueControl.Visible = true;
                _defaultValueLabel.Visible = true;
            }
            else
            {
                // keep the old widget but hide the whole default value line
                _defaultValueControl.Visible = false;
                _defaultValueLabel.Visible = false;
            }
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
                    _maxContainer.Visible = false;
                    _stepContainer.Visible = false;
                    _maxLengthContainer.Visible = false;
                    _keyValuePairContainer.Visible = false;
                    break;
                case VariableCustomizerConstraintType.MinStepMax:
                    // show the min step max parts + the spacer, hide the rest
                    _constraintSpacer.Visible = true;
                    _minStepMaxContainer.Visible = true;
                    _maxContainer.Visible = false;
                    _stepContainer.Visible = false;
                    _maxLengthContainer.Visible = false;
                    _keyValuePairContainer.Visible = false;
                    break;
                case VariableCustomizerConstraintType.Max:
                    // show the max parts + the spacer, hide the rest
                    _constraintSpacer.Visible = true;
                    _minStepMaxContainer.Visible = false;
                    _maxContainer.Visible = true;
                    _stepContainer.Visible = false;
                    _maxLengthContainer.Visible = false;
                    _keyValuePairContainer.Visible = false;
                    break;
                case VariableCustomizerConstraintType.Step:
                    // show the step parts + the spacer, hide the rest
                    _constraintSpacer.Visible = true;
                    _minStepMaxContainer.Visible = false;
                    _maxContainer.Visible = false;
                    _stepContainer.Visible = true;
                    _maxLengthContainer.Visible = false;
                    _keyValuePairContainer.Visible = false;
                    break;
                case VariableCustomizerConstraintType.MaxLength:
                    // show the max length parts + the spacer, hide the rest
                    _constraintSpacer.Visible = true;
                    _minStepMaxContainer.Visible = false;
                    _maxContainer.Visible = false;
                    _stepContainer.Visible = false;
                    _maxLengthContainer.Visible = true;
                    _keyValuePairContainer.Visible = false;
                    break;
                case VariableCustomizerConstraintType.Options:
                    // show the key value pair parts + the spacer, hide the rest
                    _constraintSpacer.Visible = true;
                    _minStepMaxContainer.Visible = false;
                    _maxContainer.Visible = false;
                    _stepContainer.Visible = false;
                    _maxLengthContainer.Visible = false;
                    _keyValuePairContainer.Visible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            RebuildLiteralWidget();
            ValidateAll();
        }

        private void OnAddKeyValuePairPressed()
        {
            AddValueLabelPair("0", "");
            // give the new  value edit the focus
            _valueLabelLines.Last().ValueEdit.GrabFocus();
        }

        private void AddValueLabelPair(string value, string label)
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

            var line = new ValueLabelLine(
                valueEdit,
                labelEdit,
                upButton,
                downButton,
                deleteButton
            );
            _valueLabelLines.Add(line);


            if (_typeHintOptionButton.SelectedPortType.IsNumericInCustomizer())
            {
                // add a numeric constraint on the values
                valueEdit.Connect("focus_exited")
                    .WithBinds(valueEdit, false, false)
                    .To(this, nameof(OnNumericFieldChanged));
            }

            // connect all edit fields for validation
            valueEdit.Connect("text_changed")
                .To(this, nameof(OnLabelValueChanged));
            labelEdit.Connect("text_changed")
                .To(this, nameof(OnLabelValueChanged));

            deleteButton.ButtonPressed += () => OnRemoveParameterPressed(line);
            upButton.ButtonPressed += () => OnParameterUpPressed(line);
            downButton.ButtonPressed += () => OnParameterDownPressed(line);

            line.InsertInto(_keyValuePairGrid);

            RepaintKeyValuePairs();
            ValidateAll();
        }

        private void OnLabelValueChanged([UsedImplicitly] string _)
        {
            RebuildLiteralWidget();
            ValidateAll();
        }

        private void FixKeyValuePairs()
        {
            // if the variable type is switched from something numeric to something non-numeric or vice versa
            // we need to remove/add all the numeric constraints from/to the value edits and maybe also need
            // to fix up the values.
            var isNumeric = _typeHintOptionButton.SelectedPortType.IsNumericInCustomizer();
            foreach (var line in _valueLabelLines)
            {
                if (isNumeric)
                {
                    // if there is currently no numeric constraint, add one
                    if (!line.ValueEdit.IsConnected("focus_exited", this, nameof(OnNumericFieldChanged)))
                    {
                        // add a numeric constraint on the values
                        line.ValueEdit.Connect("focus_exited")
                            .WithBinds(line.ValueEdit, false, false)
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
            _valueLabelLines.ForAll(it => it.RemoveFrom(_keyValuePairGrid));
            // and re-insert in correct order
            _valueLabelLines.ForAll(it => it.InsertInto(_keyValuePairGrid));
        }

        private void OnRemoveParameterPressed(ValueLabelLine line)
        {
            _valueLabelLines.Remove(line);
            line.Discard();

            RepaintKeyValuePairs();
            ValidateAll();
        }

        private void OnParameterUpPressed(ValueLabelLine line)
        {
            var index = _valueLabelLines.IndexOf(line);
            if (index <= 0)
            {
                return; // nothing to do
            }

            // swap the line with the one above. 
            var targetIndex = index - 1;
            var toMoveDown = _valueLabelLines[targetIndex];
            _valueLabelLines[targetIndex] = line;
            _valueLabelLines[index] = toMoveDown;

            RepaintKeyValuePairs();
        }

        private void OnParameterDownPressed(ValueLabelLine line)
        {
            var index = _valueLabelLines.IndexOf(line);
            if (index == -1 || index + 1 >= _valueLabelLines.Count)
            {
                return; // nothing to do
            }

            // swap the line with the one above. 
            var targetIndex = index + 1;
            var toMoveUp = _valueLabelLines[targetIndex];
            _valueLabelLines[targetIndex] = line;
            _valueLabelLines[index] = toMoveUp;

            RepaintKeyValuePairs();
        }


        private void OnNumericFieldChanged(LineEdit lineEdit, bool intOnly, bool allowEmpty)
        {
            if (allowEmpty && string.IsNullOrWhiteSpace(lineEdit.Text))
            {
                lineEdit.Text = "";
            }

            if (lineEdit.Text.SafeTryParse(out var result))
            {
                // text is numeric, allow it.
                // round to nearest int if intOnly is true
                lineEdit.Text = intOnly ? ((int) Math.Round(result)).ToString() : result.SafeToString();
            }
            else
            {
                // revert to a good default value
                lineEdit.Text = "0";
            }

            // all input fields change some constraint setting so we need to rebuild the literal widget
            // to reflect the new constraints.
            RebuildLiteralWidget();
        }


        private class ValueLabelLine
        {
            public LineEdit ValueEdit { get; }
            public LineEdit LabelEdit { get; }
            private readonly IconButton.IconButton _upButton;
            private readonly IconButton.IconButton _downButton;
            private readonly IconButton.IconButton _deleteButton;

            public ValueLabelLine(LineEdit valueEdit, LineEdit labelEdit, IconButton.IconButton upButton,
                IconButton.IconButton downButton,
                IconButton.IconButton deleteButton)
            {
                ValueEdit = valueEdit;
                LabelEdit = labelEdit;
                _upButton = upButton;
                _downButton = downButton;
                _deleteButton = deleteButton;
            }

            public void RemoveFrom(Control container)
            {
                container.RemoveChild(ValueEdit);
                container.RemoveChild(LabelEdit);
                container.RemoveChild(_upButton);
                container.RemoveChild(_downButton);
                container.RemoveChild(_deleteButton);
            }

            public void InsertInto(Control container)
            {
                container.AddChild(ValueEdit);
                container.AddChild(LabelEdit);
                container.AddChild(_upButton);
                container.AddChild(_downButton);
                container.AddChild(_deleteButton);
            }

            public void Discard()
            {
                ValueEdit.RemoveAndFree();
                LabelEdit.RemoveAndFree();
                _upButton.RemoveAndFree();
                _downButton.RemoveAndFree();
                _deleteButton.RemoveAndFree();
            }
        }
    }
}