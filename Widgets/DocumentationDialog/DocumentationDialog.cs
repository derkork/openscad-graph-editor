using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.DocumentationDialog
{
    /// <summary>
    /// Dialog for editing the documentation of an invokable.
    /// </summary>
    public class DocumentationDialog : WindowDialog
    {
        public event Action<Refactoring> RefactoringRequested;
        
        private TextEdit _descriptionEdit;
        private Control _returnValueSection;
        private LineEdit _returnValueEdit;
        private Control _parametersSection;
        private readonly List<LineEdit> _parameterEditFields = new List<LineEdit>();
        private InvokableDescription _invokableDescription;
        
        

        public override void _Ready()
        {
            _descriptionEdit = this.WithName<TextEdit>("DescriptionEdit");
            _returnValueSection = this.WithName<Control>("ReturnValueSection");
            _returnValueEdit = this.WithName<LineEdit>("ReturnValueEdit");
            _parametersSection = this.WithName<Control>("ParametersSection");

            this.WithName<Button>("OKButton")
                .Connect("pressed")
                .To(this, nameof(OnOKButtonPressed));

            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelButtonPressed));
        }


        public void Open(InvokableDescription invokableDescription)
        {
            GdAssert.That(!(invokableDescription is MainModuleDescription), "MainModuleDescription is not supported");
            
            _invokableDescription = invokableDescription;
            _descriptionEdit.Text = invokableDescription.Description;

            if (invokableDescription is FunctionDescription functionDescription)
            {
                WindowTitle = $"Documentation of function {invokableDescription.Name}";
                _returnValueEdit.Text = functionDescription.ReturnValueDescription;
                _returnValueSection.Visible = true;
            }
            else
            {
                WindowTitle = $"Documentation of module {invokableDescription.Name}";
                _returnValueSection.Visible = false;
            }
            
            _parameterEditFields.Clear();
            _parametersSection.GetChildNodes().ForAll(it => it.RemoveAndFree());
            foreach (var parameter in invokableDescription.Parameters)
            {
                var label = new Label();
                label.Text = $"Parameter {parameter.Name} ({parameter.TypeHint.HumanReadableName()})";
                _parametersSection.AddChild(label);

                var parameterEdit = new LineEdit();
                parameterEdit.Text = parameter.Description;
                _parameterEditFields.Add(parameterEdit);
                _parametersSection.AddChild(parameterEdit);
            }
            
            SetAsMinsize();
            PopupCentered();
        }
        
        private void OnOKButtonPressed()
        {
            var newDescription = _descriptionEdit.Text;
            var newParameterDescriptions = _parameterEditFields.Select(it => it.Text).ToList();

            if (_invokableDescription is FunctionDescription functionDescription)
            {
                var newReturnValueDescription = _returnValueEdit.Text;
                RefactoringRequested?.Invoke(new ChangeInvokableDocumentationRefactoring(functionDescription, newDescription, newReturnValueDescription, newParameterDescriptions));
            }
            else
            {
                RefactoringRequested?.Invoke(new ChangeInvokableDocumentationRefactoring((ModuleDescription) _invokableDescription, newDescription, newParameterDescriptions));
            }
            Hide();
        }

        private void OnCancelButtonPressed()
        {
            Hide();
        }
    }
}
