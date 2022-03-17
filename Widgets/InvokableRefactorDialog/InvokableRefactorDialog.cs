using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactoring;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.InvokableRefactorDialog
{
    [UsedImplicitly]
    public class InvokableRefactorDialog : WindowDialog
    {
        [Signal]
        public delegate void Cancelled();

        [Signal]
        public delegate void RefactoringRequested(Refactoring.Refactoring refactoring);

        private LineEdit _nameEdit;
        private Label _returnTypeLabel;
        private OptionButton _returnTypeOptionButton;
        private InvokableDescription _baseDescription;
        private LineEdit _templateParameterName;
        private OptionButton _templateParameterTypeHint;
        private Button _templateParameterUpButton;
        private Button _templateParameterDownButton;
        private Button _templateParameterDeleteButton;
        private GridContainer _parameterGrid;
        private readonly List<ParameterLine> _parameterLines = new List<ParameterLine>();
        private readonly System.Collections.Generic.Dictionary<PortType, int> _indexByPortTypes = new System.Collections.Generic.Dictionary<PortType, int>();

        private DialogMode _mode = DialogMode.Edit;

        public override void _Ready()
        {
            GetCloseButton().Visible = false;
            
            _nameEdit = this.WithName<LineEdit>("NameEdit");
            _returnTypeLabel = this.WithName<Label>("ReturnTypeLabel");
            _returnTypeOptionButton = this.WithName<OptionButton>("ReturnTypeSelector");
            
            _templateParameterName = this.WithName<LineEdit>("TemplateParameterName");
            _templateParameterName.Visible = false;
            _templateParameterTypeHint = this.WithName<OptionButton>("TemplateParameterTypeHint");
            _templateParameterTypeHint.Visible = false;

            // prepare with the known port types
            var index = 0;
            _returnTypeOptionButton.Clear();
            _templateParameterTypeHint.Clear();
            
            _templateParameterTypeHint.AddItem("Any", (int) PortType.Any);
            _returnTypeOptionButton.AddItem("Any", (int) PortType.Any);
            _indexByPortTypes[PortType.Any] = index++;
            
            _templateParameterTypeHint.AddItem("Number", (int) PortType.Number);
            _returnTypeOptionButton.AddItem("Number", (int) PortType.Number);
            _indexByPortTypes[PortType.Number] = index++;
            
            _templateParameterTypeHint.AddItem("Boolean", (int) PortType.Boolean);
            _returnTypeOptionButton.AddItem("Boolean", (int) PortType.Boolean);
            _indexByPortTypes[PortType.Boolean] = index++;
            
            _templateParameterTypeHint.AddItem("Vector3", (int) PortType.Vector3);
            _returnTypeOptionButton.AddItem("Vector3", (int) PortType.Vector3);
            _indexByPortTypes[PortType.Vector3] = index++;
            
            _templateParameterTypeHint.AddItem("String", (int) PortType.String);
            _returnTypeOptionButton.AddItem("String", (int) PortType.String);
            _indexByPortTypes[PortType.String] = index++;
            
            _templateParameterTypeHint.AddItem("Array", (int) PortType.Array);
            _returnTypeOptionButton.AddItem("Array", (int) PortType.Array);
            _indexByPortTypes[PortType.Array] = index++;
            
            _templateParameterTypeHint.AddItem("Range", (int) PortType.Range);
            _returnTypeOptionButton.AddItem("Range", (int) PortType.Range);
            _indexByPortTypes[PortType.Range] = index;

            _templateParameterUpButton = this.WithName<Button>("TemplateParameterUpButton");
            _templateParameterUpButton.Visible = false;
            _templateParameterDownButton = this.WithName<Button>("TemplateParameterDownButton");
            _templateParameterDownButton.Visible = false;
            _templateParameterDeleteButton = this.WithName<Button>("TemplateParameterDeleteButton");
            _templateParameterDeleteButton.Visible = false;
            _parameterGrid = this.WithName<GridContainer>("ParameterGrid");

            this.WithName<Button>("OkButton")
                .Connect("pressed")
                .To(this, nameof(OnOkPressed));

            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelPressed));

            this.WithName<Button>("AddParameterButton")
                .Connect("pressed")
                .To(this, nameof(OnAddParameterPressed));
        }


        public void OpenForNewFunction()
        {
            Clear();
            _mode = DialogMode.CreateFunction;
            _returnTypeLabel.Visible = true;
            _returnTypeOptionButton.Visible = true;
            _returnTypeOptionButton.Select(_indexByPortTypes[PortType.Any]);
            PopupCentered();
        }

        public void OpenForNewModule()
        {
            Clear();
            _mode = DialogMode.CreateModule;
            _returnTypeLabel.Visible = false;
            _returnTypeOptionButton.Visible = false;
            PopupCentered();
        }

        public void Open(InvokableDescription description)
        {
            Clear();
            _baseDescription = description;
            _mode = DialogMode.Edit;

            _nameEdit.Text = description.Name;
            if (description is FunctionDescription functionDescription)
            {
                _returnTypeLabel.Visible = true;
                _returnTypeOptionButton.Visible = true;
                _returnTypeOptionButton.Select(_indexByPortTypes[functionDescription.ReturnTypeHint]);
            }
            else
            {
                _returnTypeLabel.Visible = false;
                _returnTypeOptionButton.Visible = false;
            }

            foreach (var parameter in description.Parameters)
            {
                AddParameter(parameter.Name, parameter.TypeHint);
            }
            
            PopupCentered();
        }

        private void Clear()
        {
            _baseDescription = null;
            _parameterLines.ForAll(it => it.Discard());
            _parameterLines.Clear();
        }

        private void OnOkPressed()
        {
            switch (_mode)
            {
                case DialogMode.Edit:
                    break;
                case DialogMode.CreateFunction:
                    var functionDescription = FunctionBuilder
                        .NewFunction(_nameEdit.Text, returnType: (PortType) _returnTypeOptionButton.GetSelectedId());
                    foreach (var line in _parameterLines)
                    {
                        functionDescription.WithParameter(line.Name, line.TypeHint);
                    }
                    EmitSignal(nameof(RefactoringRequested), new IntroduceInvokableRefactoring(functionDescription.Build()));
                    break;

                case DialogMode.CreateModule:
                    var moduleDescription = ModuleBuilder
                        .NewModule(_nameEdit.Text);
                        
                    foreach (var line in _parameterLines)
                    {
                        moduleDescription.WithParameter(line.Name, line.TypeHint);
                    }

                    EmitSignal(nameof(RefactoringRequested), new IntroduceInvokableRefactoring(moduleDescription.Build()));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Hide();
        }

        private void OnCancelPressed()
        {
            EmitSignal(nameof(Cancelled));
            Hide();
        }

        private void OnAddParameterPressed()
        {
            AddParameter("parameter" + _parameterLines.Count, PortType.Any);
        }

        private void AddParameter(string name, PortType typeHint)
        {
            var nameEdit = _templateParameterName.Clone();
            var optionButton = _templateParameterTypeHint.Clone();
            var deleteButton = _templateParameterDeleteButton.Clone();
            var upButton = _templateParameterUpButton.Clone();
            var downButton = _templateParameterDownButton.Clone();

            nameEdit.Visible = true;
            optionButton.Visible = true;
            deleteButton.Visible = true;
            upButton.Visible = true;
            downButton.Visible = true;
            
            nameEdit.Text = name;
            optionButton.Select(_indexByPortTypes[typeHint]);

            var line = new ParameterLine(
                nameEdit,
                optionButton,
                upButton,
                downButton,
                deleteButton
            );
            _parameterLines.Add(line);

            deleteButton.Connect("pressed")
                .WithBinds(line.HoldMyBeer())
                .To(this, nameof(OnRemoveParameterPressed));

            upButton.Connect("pressed")
                .WithBinds(line.HoldMyBeer())
                .To(this, nameof(OnParameterUpPressed));

            downButton.Connect("pressed")
                .WithBinds(line.HoldMyBeer())
                .To(this, nameof(OnParameterDownPressed));

            line.InsertInto(_parameterGrid);

            Repaint();
        }

        private void Repaint()
        {
            // remove all parameters
            _parameterLines.ForAll(it => it.RemoveFrom(_parameterGrid));
            // and re-insert in correct order
            _parameterLines.ForAll(it => it.InsertInto(_parameterGrid));
        }

        private void OnRemoveParameterPressed(Reference lineReference)
        {
            if (!lineReference.TryGetBeer<ParameterLine>(out var line))
            {
                return;
            }

            _parameterLines.Remove(line);
            line.Discard();

            Repaint();
        }

        private void OnParameterUpPressed(Reference lineReference)
        {
            if (!lineReference.TryGetBeer<ParameterLine>(out var line))
            {
                return;
            }

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

        private void OnParameterDownPressed(Reference lineReference)
        {
            if (!lineReference.TryGetBeer<ParameterLine>(out var line))
            {
                return;
            }

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
            private readonly LineEdit _nameEdit;
            private readonly OptionButton _typeHint;
            private readonly Button _upButton;
            private readonly Button _downButton;
            private readonly Button _deleteButton;

            public ParameterLine(LineEdit nameEdit, OptionButton typeHint, Button upButton, Button downButton,
                Button deleteButton)
            {
                _nameEdit = nameEdit;
                _typeHint = typeHint;
                _upButton = upButton;
                _downButton = downButton;
                _deleteButton = deleteButton;
            }

            public string Name => _nameEdit.Text;
            public PortType TypeHint => (PortType) _typeHint.GetSelectedId();

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