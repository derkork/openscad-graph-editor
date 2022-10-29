using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.VariableCustomizer
{
    [UsedImplicitly]
    public class VariableCustomizer : MarginContainer
    {

        public event Action<string, Refactoring> RefactoringRequested;
        public event Action<VariableDescription> VariableEditingRequested;

        private Control _sectionTemplate;
        private Control _variableTemplate;
        
        private Control _sectionsContainer;

        private readonly Dictionary<string, Control> _sections = new Dictionary<string, Control>();
        private readonly Dictionary<string, Control> _variables = new Dictionary<string, Control>();
        private readonly Dictionary<string, VariableDescription> _variablesByName = new Dictionary<string, VariableDescription>();

        public override void _Ready()
        {
            // get the templates and remove them from the tree so they don't render
            _sectionTemplate = this.WithName<Control>("SectionTemplate");
            _variableTemplate = this.WithName<Control>("VariableTemplate");
            
            _sectionTemplate.RemoveFromParent();
            _variableTemplate.RemoveFromParent();
            
            _sectionsContainer = this.WithName<Control>("Sections");
        }

        public void Setup(List<VariableDescription> variables)
        {
            // fill the variables by name dictionary
            _variablesByName.Clear();
            foreach (var variable in variables)
            {
                _variablesByName.Add(variable.Name, variable);
            }
            
            // first get all the variables from the project and group them by section
            var variableGroups = variables
                // only variables in the customizer
                .Where(it => it.ShowInCustomizer)
                .GroupBy(v => v.CustomizerDescription.Tab)
                .ToList();

            foreach (var variableGroup in variableGroups)
            {
                var sectionName = variableGroup.Key;
                if (sectionName == "")
                {
                    sectionName = "Parameters";
                }
                var variablesInSection = variableGroup.ToList();
                
                // get the section control
                var section = CreateOrGetSection(sectionName);
                var sectionContents = section.WithName<Control>("SectionContents");
                
                // build all variables in the section
                foreach (var variable in variablesInSection)
                {
                    var variableControl = CreateOrGetVariable(variable);
                    
                    // move it below the section
                    variableControl.MoveToNewParent(sectionContents);
                }
            }
            
            // kill all unused variables
            var unusedVariables = _variables.Keys.Except(variables.Select(it => it.Name)).ToList();
            foreach (var unusedVariable in unusedVariables)
            {
                _variables[unusedVariable].RemoveAndFree();
                _variables.Remove(unusedVariable);
            }
            
            // kill all unused sections
            var unusedSections = _sections.Keys
                .Except(variableGroups.Select(it => it .Key == "" ? "Parameters" : it.Key))
                .ToList();
            foreach (var unusedSection in unusedSections)
            {
                _sections[unusedSection].RemoveAndFree();
                _sections.Remove(unusedSection);
            }
        }

        
        private Control CreateOrGetSection(string name)
        {
            if (_sections.TryGetValue(name, out var result))
            {
                return result;
            }
            
            // create a clone of the section template
            result = _sectionTemplate.Duplicate() as Control;
            
            // set the name
            result.WithName<Label>("SectionName").Text = name;
            
            // get the toggle button
            var toggle = result.WithName<IconButton.IconButton>("SectionToggle");
            
            // toggle section visibility when clicked
            var insertPoint = result.WithName<Control>("SectionContents");
            toggle.ButtonToggled += (pressed) =>
            {
                insertPoint.Visible = pressed;
            };
            
            // add the section to the tree
            _sectionsContainer.AddChild(result);
            
            // add the section to the dictionary
            _sections.Add(name, result);
            
            return result;
        }
        
        private Control CreateOrGetVariable(VariableDescription variable)
        {
            // if we have this already, use it, otherwise create a new one from the template
            if (!_variables.TryGetValue(variable.Name, out var result))
            {
                result = _variableTemplate.Duplicate() as Control;
                // set the name
                var nameLabel = result.WithName<RichTextLabel>("VariableName");
                // we take advantage of the fact that the variable name cannot possibly contain bbcode.
                nameLabel.Connect("meta_clicked")
                    .WithBinds(variable.Name)
                    .To(this, nameof(OnVariableClicked));
                    
                nameLabel.BbcodeText = $"[url]{variable.Name}[/url]";
                // add the variable to the dictionary
                _variables.Add(variable.Name, result);
            }
            
            // set/update the description
            result.WithName<Label>("VariableDescription").Text = variable.Description;
            
            // get the control for the variable
            var variableControl = result.WithName<Control>("VariableControl");
            
            // build an updated control for the variable
            var literal = variable.DefaultValue;
            if (literal != null)
            {
                // make a copy of the literal
                var copy = literal.LiteralType.BuildLiteral(literal.SerializedValue);
                
                // build a control that is bound to the copy
                var newWidget = copy.BuildWidget(false, true, false, variableControl as IScadLiteralWidget, variable.CustomizerDescription);
                if (newWidget != null && newWidget != variableControl)
                {
                    // replace the old widget with the new one
                    var parent = variableControl.GetParent();
                    // destroy the old one
                    variableControl.RemoveAndFree();
                    // set the name so we can find it again
                    ((Control) newWidget).Name = "VariableControl";
                    ((Control) newWidget).MoveToNewParent(parent);
                    // make it fill horizontally
                    ((Control) newWidget).SizeFlagsHorizontal = (int) SizeFlags.ExpandFill;
                    // add a handler for the value change
                    newWidget.LiteralValueChanged += (newLiteral) =>
                    {
                        if (newLiteral.SerializedValue != literal.SerializedValue)
                        {
                            // when the value changes, request a refactoring
                            RefactoringRequested?.Invoke($"Change default value of {variable.Name}",
                                new ChangeVariableDefaultValueRefactoring(variable, newLiteral));
                        }
                    };
                }
            }
            else
            {
                // hide the control
                variableControl.Visible = false;
            }

            return result;
        }
        
        private void OnVariableClicked([UsedImplicitly]object _, string variableName)
        {
            // when the variable name is clicked, request the variable editing dialog
            VariableEditingRequested?.Invoke(_variablesByName[variableName]);
        }
    }
}