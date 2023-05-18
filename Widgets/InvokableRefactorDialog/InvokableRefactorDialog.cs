using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.InvokableRefactorDialog
{
    [UsedImplicitly]
    public class InvokableRefactorDialog : WindowDialog
    {
        private LineEdit _nameEdit;
        private Label _returnTypeLabel;
        private PortTypeSelector _returnTypeOptionButton;
        private InvokableDescription _baseDescription;
        private LineEdit _templateParameterName;
        private PortTypeSelector _templateParameterTypeHint;
        private IconButton.IconButton _templateParameterUpButton;
        private IconButton.IconButton _templateParameterDownButton;
        private IconButton.IconButton _templateParameterDeleteButton;
        private MarginContainer _rootContainer;
        private Label _errorLabel;
        private GridContainer _parameterGrid;
        private readonly List<ParameterLine> _parameterLines = new List<ParameterLine>();

        private DialogMode _mode = DialogMode.Edit;
        private Button _okButton;
        private IEditorContext _context;

        public override void _Ready()
        {
            GetCloseButton().Visible = false;
            _rootContainer = this.WithName<MarginContainer>("RootContainer");
            _nameEdit = this.WithName<LineEdit>("NameEdit");
            _nameEdit
                .Connect("text_changed")
                .To(this, nameof(OnIdentifierChanged));
            _returnTypeLabel = this.WithName<Label>("ReturnTypeLabel");
            _returnTypeOptionButton = this.WithName<PortTypeSelector>("ReturnTypeSelector");

            _errorLabel = this.WithName<Label>("ErrorLabel");

            _templateParameterName = this.WithName<LineEdit>("TemplateParameterName");
            _templateParameterName.Visible = false;
            _templateParameterTypeHint = this.WithName<PortTypeSelector>("TemplateParameterTypeHint");
            _templateParameterTypeHint.Visible = false;

            _templateParameterUpButton = this.WithName<IconButton.IconButton>("TemplateParameterUpButton");
            _templateParameterUpButton.Visible = false;
            _templateParameterDownButton = this.WithName<IconButton.IconButton>("TemplateParameterDownButton");
            _templateParameterDownButton.Visible = false;
            _templateParameterDeleteButton = this.WithName<IconButton.IconButton>("TemplateParameterDeleteButton");
            _templateParameterDeleteButton.Visible = false;
            _parameterGrid = this.WithName<GridContainer>("ParameterGrid");

            _okButton = this.WithName<Button>("OkButton");
            _okButton
                .Connect("pressed")
                .To(this, nameof(OnOkPressed));

            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelPressed));

            this.WithName<IconButton.IconButton>("AddParameterButton").ButtonPressed += OnAddParameterPressed;
        }


        public void OpenForNewFunction(IEditorContext context)
        {
            Clear();
            _context = context;
            WindowTitle = "New Function";
            _mode = DialogMode.CreateFunction;
            _returnTypeLabel.Visible = true;
            _returnTypeOptionButton.Visible = true;
            _returnTypeOptionButton.SelectedPortType = PortType.Any;
            ValidateAll();
            SetAsMinsize();
            PopupCentered();
        }

        public void OpenForNewModule(IEditorContext context)
        {
            Clear();
            _context = context;
            WindowTitle = "New Module";
            _mode = DialogMode.CreateModule;
            _returnTypeLabel.Visible = false;
            _returnTypeOptionButton.Visible = false;
            ValidateAll();
            PopupCentered();
        }

        public void OpenForEditing(IEditorContext context, InvokableDescription description)
        {
            Clear();
            _context = context;
            WindowTitle = $"Refactor {description.Name}";
            _baseDescription = description;
            _mode = DialogMode.Edit;

            _nameEdit.Text = description.Name;
            switch (description)
            {
                case FunctionDescription functionDescription:
                    _returnTypeLabel.Visible = true;
                    _returnTypeOptionButton.Visible = true;
                    _returnTypeOptionButton.SelectedPortType = functionDescription.ReturnTypeHint;
                    break;
                case ModuleDescription _:
                    _returnTypeLabel.Visible = false;
                    _returnTypeOptionButton.Visible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (var index = 0; index < description.Parameters.Count; index++)
            {
                var parameter = description.Parameters[index];
                AddParameter(parameter.Name, parameter.TypeHint, parameter.Name, (int) parameter.TypeHint, index);
            }

            ValidateAll();
            PopupCentered();
        }

        private void Clear()
        {
            _baseDescription = null;
            _context = null;
            _nameEdit.Text = "";
            _parameterLines.ForAll(it => it.Discard());
            _parameterLines.Clear();
            _okButton.Disabled = true;
        }

        private void OnOkPressed()
        {
            _okButton.Disabled = true;

            var refactorings = new List<Refactoring>();

            switch (_mode)
            {
                case DialogMode.Edit:
                    // check if the name has changed
                    if (_baseDescription.Name != _nameEdit.Text)
                    {
                        refactorings.Add(new RenameInvokableRefactoring(_baseDescription, _nameEdit.Text));
                    }

                    // now analyze the parameters and see what has changed.
                    // First create a refactoring to delete all parameters that were deleted.
                    var indicesToDelete = _baseDescription.Parameters.Indices()
                        // ReSharper disable once SimplifyLinqExpressionUseAll
                        // all indices that are no longer referenced in the _parameterLines list
                        .Where(it => !_parameterLines.Any(line => line.OriginalIndex == it))
                        .ToArray();

                    if (indicesToDelete.Length > 0)
                    {
                        refactorings.Add(new DeleteInvokableParametersRefactoring(_baseDescription, indicesToDelete));
                    }


                    // now check if any new parameters have been added
                    var parametersToAdd = _parameterLines
                        .Where(it => it.OriginalName == null)
                        .Select(it =>
                        {
                            var parameterDescription = new ParameterDescription
                            {
                                Name = it.Name,
                                TypeHint = it.TypeHint
                            };
                            return parameterDescription;
                        })
                        .ToArray();

                    if (parametersToAdd.Length > 0)
                    {
                        refactorings.Add(new AddInvokableParametersRefactoring(_baseDescription, parametersToAdd));
                    }


                    // now check if any parameters have changed their type
                    _parameterLines
                        .Where(it => it.OriginalPortType != -1 && it.OriginalPortType != (int) it.TypeHint)
                        .Select(it =>
                            new ChangeInvokableParameterTypeRefactoring(_baseDescription, it.OriginalName, it.TypeHint))
                        .ForAll(it => refactorings.Add(it));
                    
                    // once these refactorings have been created, we can now check if any parameters have changed
                    // their name. It is important we do rename AFTER changing type as the type change refactoring 
                    // refers to the name of the parameters.
                    _parameterLines
                        .Where(it => it.OriginalName != null && it.OriginalName != it.Name)
                        .Select(it =>
                            new RenameInvokableParameterRefactoring(_baseDescription, it.OriginalName, it.Name))
                        .ForAll(it => refactorings.Add(it));

                    // also check if the return type has changed in case we are editing a function
                    if (_baseDescription is FunctionDescription aFunctionDescription)
                    {
                        var selectedReturnType = _returnTypeOptionButton.SelectedPortType;
                        if (aFunctionDescription.ReturnTypeHint != selectedReturnType)
                        {
                            refactorings.Add(
                                new ChangeFunctionReturnTypeRefactoring(aFunctionDescription, selectedReturnType));
                        }
                    }

                    // finally check if any parameters have been reordered
                    // if the number of parameters differs, just reorder all of them
                    if (_parameterLines.Count != _baseDescription.Parameters.Count)
                    {
                        refactorings.Add(new ChangeParameterOrderRefactoring(_baseDescription,
                            _parameterLines.Select(it => it.Name).ToArray()));
                    }
                    else
                    {
                        // we have the same amount of parameters so check if one of them has changed its order
                        if (_baseDescription.Parameters.Where((t, i) => t.Name != _parameterLines[i].Name).Any())
                        {
                            refactorings.Add(new ChangeParameterOrderRefactoring(_baseDescription,
                                _parameterLines.Select(it => it.Name).ToArray()));
                        }
                    }

                    break;
                case DialogMode.CreateFunction:
                    var functionDescription = FunctionBuilder
                        .NewFunction(_nameEdit.Text, returnType: _returnTypeOptionButton.SelectedPortType);
                    foreach (var line in _parameterLines)
                    {
                        functionDescription.WithParameter(line.Name, line.TypeHint);
                    }

                    // we need this down there when calling our listeners
                    _baseDescription = functionDescription.Build();
                    refactorings.Add(new IntroduceInvokableRefactoring(_baseDescription));
                    break;

                case DialogMode.CreateModule:
                    var moduleDescription = ModuleBuilder
                        .NewModule(_nameEdit.Text);

                    foreach (var line in _parameterLines)
                    {
                        moduleDescription.WithParameter(line.Name, line.TypeHint);
                    }

                    // we need this down there when calling our listeners
                    _baseDescription = moduleDescription.Build();
                    refactorings.Add(new IntroduceInvokableRefactoring(_baseDescription));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (refactorings.Count > 0)
            {
                switch (_mode)
                {
                    case DialogMode.Edit:
                        _context.PerformRefactorings($"Change {_baseDescription.Name}", refactorings);
                        break;
                    case DialogMode.CreateFunction:
                    case DialogMode.CreateModule:
                        _context.PerformRefactorings($"Create {_baseDescription.Name}", refactorings);
                        // then open the newly created function
                        _context.OpenGraph(_baseDescription);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
        }

        private void ValidateAll()
        {
            ValidityChecker.For(_errorLabel, _okButton)
                .Check(
                    // we create a function, then the name must be different from all other functions in the project
                    (_mode == DialogMode.CreateFunction && _context.CurrentProject.Functions.Select(it => it.Description.Name).All(it => it != _nameEdit.Text))
                    // we create a module, then the name must be different from all other modules in the project
                    || (_mode == DialogMode.CreateModule && _context.CurrentProject.Modules.Select(it => it.Description.Name).All(it => it != _nameEdit.Text))
                    // we edit a function, then the name must be different from all other functions in the project
                    || (_mode == DialogMode.Edit && _baseDescription is FunctionDescription && _context.CurrentProject.Functions.Where(it => it.Description != _baseDescription).Select(it => it.Description.Name).All(it => it != _nameEdit.Text))
                    // we edit a module, then the name must be different from all other modules in the project
                    || (_mode == DialogMode.Edit && _baseDescription is ModuleDescription && _context.CurrentProject.Modules.Where(it => it.Description != _baseDescription).Select(it => it.Description.Name).All(it => it != _nameEdit.Text))
                    , "The name is already used in this project.")
                .Check(
                    _nameEdit.Text.IsValidIdentifier(),
                    $"Name must not be blank and must be only letters, numbers, and underscores and must not start with a number."
                )
                .CheckAll(
                    _parameterLines,
                    it => it.Name.IsValidIdentifier(),
                    it =>
                        $"Parameter name '{it.Name}' must be only letters, numbers, and underscores and must not start with a number."
                )
                .Check(
                    _parameterLines.Select(it => it.Name).Distinct().Count() == _parameterLines.Count,
                    "Parameter names must be unique."
                )
                .UpdateUserInterface();
            CallDeferred(nameof(FixSize));
        }

        private void FixSize()
        {
            // this is a crappy hack to get the dialog to fit the content.
            SetSize(_rootContainer.RectSize);
        }
        
        private void OnAddParameterPressed()
        {
            AddParameter("parameter" + _parameterLines.Count, PortType.Any);
        }

        private void AddParameter(string name, PortType typeHint, string originalName = null, int originalPortType = -1,
            int originalIndex = -1)
        {
            var parameterUuid = Guid.NewGuid().ToString();
            // while we technically don't need it, we give every new item a unique name using the uuid and a string
            // this makes it easier for the test driver to attach to it.
            var nameEdit = _templateParameterName.Clone();
            nameEdit.Name = "name-" + parameterUuid;
            var optionButton = _templateParameterTypeHint.Clone();
            optionButton.Name = "type-" + parameterUuid;
            var deleteButton = _templateParameterDeleteButton.Clone();
            deleteButton.Name = "delete-" + parameterUuid;
            var upButton = _templateParameterUpButton.Clone();
            upButton.Name = "up-" + parameterUuid;
            var downButton = _templateParameterDownButton.Clone();
            downButton.Name = "down-" + parameterUuid;

            nameEdit.Visible = true;
            optionButton.Visible = true;
            deleteButton.Visible = true;
            upButton.Visible = true;
            downButton.Visible = true;

            nameEdit.Text = name;
            optionButton.SelectedPortType = typeHint;

            var line = new ParameterLine(
                nameEdit,
                optionButton,
                upButton,
                downButton,
                deleteButton,
                originalName,
                originalPortType,
                originalIndex
            );
            _parameterLines.Add(line);

            nameEdit.Connect("text_changed")
                .To(this, nameof(OnIdentifierChanged));

            deleteButton.ButtonPressed += () => OnRemoveParameterPressed(line);

            upButton.ButtonPressed += () => OnParameterUpPressed(line);

            downButton.ButtonPressed += () => OnParameterDownPressed(line);

            line.InsertInto(_parameterGrid);

            Repaint();
            ValidateAll();
            SetAsMinsize();
            
            // give the new  name edit the focus
            nameEdit.GrabFocus();
        }

        private void Repaint()
        {
            // remove all parameters
            _parameterLines.ForAll(it => it.RemoveFrom(_parameterGrid));
            // and re-insert in correct order
            _parameterLines.ForAll(it => it.InsertInto(_parameterGrid));
        }

        private void OnRemoveParameterPressed(ParameterLine line)
        {
            _parameterLines.Remove(line);
            line.Discard();

            Repaint();
        }

        private void OnParameterUpPressed(ParameterLine line)
        {
            var index = _parameterLines.IndexOf(line);
            if (index <= 0)
            {
                return; // nothing to do
            }

            // swap the line with the one above. 
            var targetIndex = index - 1;
            var toMoveDown = _parameterLines[targetIndex];
            _parameterLines[targetIndex] = line;
            _parameterLines[index] = toMoveDown;

            Repaint();
        }

        private void OnParameterDownPressed(ParameterLine line)
        {
            var index = _parameterLines.IndexOf(line);
            if (index == -1 || index + 1 >= _parameterLines.Count)
            {
                return; // nothing to do
            }

            // swap the line with the one above. 
            var targetIndex = index + 1;
            var toMoveUp = _parameterLines[targetIndex];
            _parameterLines[targetIndex] = line;
            _parameterLines[index] = toMoveUp;

            Repaint();
        }

        private class ParameterLine
        {
            /// <summary>
            /// The name this parameter had before the refactoring. If null, this parameter did not exist before.
            /// </summary>
            public string OriginalName { get; }

            /// <summary>
            /// The port type this parameter had before the refactoring. If -1, this parameter did not exist before.
            /// </summary>
            public int OriginalPortType { get; }

            /// <summary>
            /// The index this parameter had before the refactoring. If -1, this parameter did not exist before.
            /// </summary>
            public int OriginalIndex { get; }

            private readonly LineEdit _nameEdit;
            private readonly PortTypeSelector _typeHint;
            private readonly IconButton.IconButton _upButton;
            private readonly IconButton.IconButton _downButton;
            private readonly IconButton.IconButton _deleteButton;

            public ParameterLine(LineEdit nameEdit, PortTypeSelector typeHint, IconButton.IconButton upButton, IconButton.IconButton downButton,
                IconButton.IconButton deleteButton, string originalName = null, int originalPortType = -1, int originalIndex = -1)
            {
                OriginalName = originalName;
                OriginalPortType = originalPortType;
                OriginalIndex = originalIndex;
                _nameEdit = nameEdit;
                _typeHint = typeHint;
                _upButton = upButton;
                _downButton = downButton;
                _deleteButton = deleteButton;
            }

            public string Name => _nameEdit.Text;
            public PortType TypeHint => _typeHint.SelectedPortType;

            public void RemoveFrom(Control container)
            {
                container.RemoveChild(_nameEdit);
                container.RemoveChild(_typeHint);
                container.RemoveChild(_upButton);
                container.RemoveChild(_downButton);
                container.RemoveChild(_deleteButton);
            }

            public void InsertInto(Control container)
            {
                container.AddChild(_nameEdit);
                container.AddChild(_typeHint);
                container.AddChild(_upButton);
                container.AddChild(_downButton);
                container.AddChild(_deleteButton);
            }

            public void Discard()
            {
                _nameEdit.RemoveAndFree();
                _typeHint.RemoveAndFree();
                _upButton.RemoveAndFree();
                _downButton.RemoveAndFree();
                _deleteButton.RemoveAndFree();
            }
        }
    }
}