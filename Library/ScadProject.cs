using System;
using System.Collections.Generic;
using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    public class ScadProject : IReferenceResolver
    {
        private readonly IReferenceResolver _parentResolver;

        private readonly Dictionary<string, FunctionDescription> _projectFunctionDescriptions =
            new Dictionary<string, FunctionDescription>();

        private readonly Dictionary<string, ModuleDescription> _projectModuleDescriptions =
            new Dictionary<string, ModuleDescription>();

        private readonly Dictionary<string, VariableDescription> _projectVariables =
            new Dictionary<string, VariableDescription>();

        private readonly Dictionary<string, ExternalReference> _externalReferences = new Dictionary<string, ExternalReference>();

        private readonly HashSet<ScadGraph> _modules = new HashSet<ScadGraph>();
        private readonly HashSet<ScadGraph> _functions = new HashSet<ScadGraph>();

        public IEnumerable<ScadGraph> Modules => _modules.OrderBy(x => x.Description.Name);
        public IEnumerable<ScadGraph> Functions => _functions.OrderBy(x => x.Description.Name);

        public IEnumerable<ScadGraph> AllDeclaredInvokables => Functions.Concat(Modules).Append(MainModule);

        public IEnumerable<VariableDescription> Variables => _projectVariables.Values.OrderBy(x => x.Name);
        public IEnumerable<ExternalReference> ExternalReferences => _externalReferences.Values.OrderBy(x => x.IncludePath);

        public ScadGraph MainModule { get; private set; }

        public string ProjectPath { get; private set; }
        public string Preamble { get; set; } = "";

        public ScadProject(IReferenceResolver parentResolver)
        {
            _parentResolver = parentResolver;
            var mainModuleGraph = new ScadGraph();
            mainModuleGraph.Main();
            MainModule = mainModuleGraph;
        }

        public FunctionDescription ResolveFunctionReference(string id)
        {
            if (_projectFunctionDescriptions.TryGetValue(id, out var functionDescription))
            {
                return functionDescription;
            }

            var fromExternal = _externalReferences
                .Values
                .SelectMany(it => it.Functions)
                .FirstOrDefault(it => it.Id == id);
            
            return fromExternal ?? _parentResolver.ResolveFunctionReference(id);
        }

        public ModuleDescription ResolveModuleReference(string id)
        {
            if (_projectModuleDescriptions.TryGetValue(id, out var moduleDescription))
            {
                return moduleDescription;
            }
            
            var fromExternal = _externalReferences
                .Values
                .SelectMany(it => it.Modules)
                .FirstOrDefault(it => it.Id == id);

            return fromExternal ?? _parentResolver.ResolveModuleReference(id);
        }

        public VariableDescription ResolveVariableReference(string id)
        {
            if (_projectVariables.TryGetValue(id, out var variableDescription))
            {
                return variableDescription;
            }
            
            var fromExternal = _externalReferences
                .Values
                .SelectMany(it => it.Variables)
                .FirstOrDefault(it => it.Id == id);
            

            return fromExternal ?? _parentResolver.ResolveVariableReference(id);
        }

        public ExternalReference ResolveExternalReference(string id)
        {
            if (_externalReferences.TryGetValue(id, out var result))
            {
                return result;
            }
            
            return _parentResolver.ResolveExternalReference(id);
        }

        private void Clear()
        {
            _projectFunctionDescriptions.Clear();
            _projectModuleDescriptions.Clear();
            _projectVariables.Clear();

            MainModule = null;

            _modules.Clear();
            _functions.Clear();
            _externalReferences.Clear();
        }

        public void Load(SavedProject project, string projectPath)
        {
            Clear();
            ProjectPath = projectPath;
            // Step 1: load function descriptions so we can resolve them in step 4
            foreach (var function in project.Functions)
            {
                _projectFunctionDescriptions[function.Description.Id] = 
                    (FunctionDescription)  function.Description.FromSavedState();
            }

            foreach (var module in project.Modules)
            {
                _projectModuleDescriptions[module.Description.Id] =
                    (ModuleDescription) module.Description.FromSavedState();
            }

            // Step 2: load variable descriptions so we can resolve them in step 4
            foreach (var variable in project.Variables)
            {
                _projectVariables.Add(variable.Id, variable.FromSavedState());
            }

            // Step 3: load external references so we can resolve them in step 4
            foreach (var external in project.ExternalReferences)
            {
                _externalReferences.Add(external.Id, external.FromSavedState());
            }

            // Step 4: load the actual graphs, which can now resolve references to other functions.
            foreach (var function in project.Functions)
            {
                var functionContext = new ScadGraph();
                functionContext.LoadFrom(function, _projectFunctionDescriptions[function.Description.Id], this);
                _functions.Add(functionContext);
            }

            foreach (var module in project.Modules)
            {
                var moduleContext = new ScadGraph();
                moduleContext.LoadFrom(module, _projectModuleDescriptions[module.Description.Id], this);
                _modules.Add(moduleContext);
            }

            // Step 5: load the main module
            MainModule = new ScadGraph();
            MainModule.LoadFrom(project.MainModule, project.MainModule.Description.FromSavedState(), this);
            
            // Step 6: load preamble
            Preamble = project.Preamble;
        }

        public SavedProject Save()
        {
            var result = Prefabs.New<SavedProject>();
            
            result.Preamble = Preamble;

            _externalReferences.ForAll(it => result.ExternalReferences.Add(it.Value.ToSavedState()));

            foreach (var function in _functions)
            {
                result.Functions.Add(function.ToSavedState());
            }

            foreach (var module in _modules)
            {
                result.Modules.Add(module.ToSavedState());
            }

            foreach (var variable in _projectVariables.Values)
            {
                result.Variables.Add(variable.ToSavedState());
            }

            {
                result.MainModule = MainModule.ToSavedState();
            }

            return result;
        }

        public string Render()
        {
            return string.Join("\n",
                // first the preamble
                new [] {Preamble}
                    // then any include statements
                    .Concat(_externalReferences.Select(it => it.Value.Render()))
                    // then customized variables
                    .Concat(Variables.Select(RenderCustomizedVariable))
                    // render a dummy module to prevent non customizer variables
                    // from appearing in the customizer
                    .Concat(new[] {"module __Customizer_Limit__ () {}"})
                    // then non-customized variables
                    .Concat(Variables.Select(RenderNonCustomizedVariableInitializer))
                    // modules and functions
                    .Concat(_modules.Select(it => it.Render()))
                    .Concat(_functions.Select(it => it.Render()))
                    // finally the main module
                    .Append(MainModule.Render())
                    .Where(it => it.Length > 0)
            );
        }

  

        private string RenderCustomizedVariable(VariableDescription variableDescription)
        {
            if (!variableDescription.ShowInCustomizer)
            {
                return "";
            }

            var constraint = RenderCustomizerConstraint(variableDescription);
            var description = variableDescription.Description.Length > 0 ? $"// {variableDescription.Description}\n" : "";
            // the tab name must be sanitized so it doesn't break the comment and tab notation syntax
            // we strip out all occurrences of '*/' and '[' and ']' and replace them with
            // underscores.
            var tabName = variableDescription.CustomizerDescription.Tab
                .Replace("*/", "__")
                .Replace("[", "_")
                .Replace("]", "_");
            
            var tab = tabName.Length > 0 ? $"/* [{tabName}] */\n" : "";
            var defaultValue = variableDescription.DefaultValue?.RenderedValue ?? "0";
            
            return $"{tab}{description}{variableDescription.Name} = {defaultValue}; {constraint}\n";
        }

        private string RenderCustomizerConstraint(VariableDescription variableDescription)
        {
            var customizerDescription = variableDescription.CustomizerDescription;
            
            switch (customizerDescription.ConstraintType)
            {
                case VariableCustomizerConstraintType.None:
                    return "";
                case VariableCustomizerConstraintType.MinStepMax:
                    var min =  customizerDescription.Min;
                    var max = customizerDescription.Max;
                    var step = customizerDescription.Step;
                    
                    // then either render a min/max
                    if (step.Length > 0)
                    {
                        // render a min/step/max
                        return $"// [{min}:{step}:{max}]";
                    }
                    
                    // render a min/max
                    return $"// [{min}:{max}]";
                case VariableCustomizerConstraintType.Max:
                    // render a simplified max
                    return $"// [{customizerDescription.Max}]";
                case VariableCustomizerConstraintType.Step:
                    // render a step, only
                    return $"// {customizerDescription.Step}";
                case VariableCustomizerConstraintType.MaxLength:
                    // max length is very easy.
                    return $"// {customizerDescription.MaxLength}";
                case VariableCustomizerConstraintType.Options:
                    // and finally the value-label-pairs. we always render both value and label. if the label is empty
                    // we render the value as label.
                    if (customizerDescription.ValueLabelPairs.Count == 0)
                    {
                        return "";
                    }
                    
                    var pairs = customizerDescription.ValueLabelPairs.Select(it =>
                    {
                        var value = it.Value.RenderedValue;
                        // use the value as label if the label is empty
                        var label = it.Label.Value.Length >0 ? it.Label.RenderedValue : value;
                        return $"{value}:{label}";
                    });
                    return $"// [{string.Join(",", pairs)}]";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string RenderNonCustomizedVariableInitializer(VariableDescription variable)
        {
            // we only render non-customized variables that have a default value
            if (variable.ShowInCustomizer || variable.DefaultValue == null)
            {
                return "";
            }
            
            return $"{variable.Name} = {variable.DefaultValue.RenderedValue};";
        }

        public ScadGraph AddInvokable(InvokableDescription invokableDescription)
        {
            var graph = new ScadGraph();
            graph.NewFromDescription(invokableDescription);
            switch (invokableDescription)
            {
                case FunctionDescription functionDescription:
                    _functions.Add(graph);
                    _projectFunctionDescriptions[functionDescription.Id] = functionDescription;
                    break;
                case ModuleDescription moduleDescription:
                    _modules.Add(graph);
                    _projectModuleDescriptions[moduleDescription.Id] = moduleDescription;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return graph;
        }

        public void RemoveInvokable(InvokableDescription description)
        {
            var graph = FindDefiningGraph(description);
            switch (description)
            {
                case FunctionDescription _:
                    _functions.Remove(graph);
                    _projectFunctionDescriptions.Remove(description.Id);
                    break;
                case ModuleDescription _:
                    _modules.Remove(graph);
                    _projectModuleDescriptions.Remove(description.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void AddVariable(VariableDescription variableDescription)
        {
            _projectVariables[variableDescription.Id] = variableDescription;
        }

        public void RemoveVariable(VariableDescription variableDescription)
        {
            _projectVariables.Remove(variableDescription.Id);
        }

        public ScadGraph FindDefiningGraph(InvokableDescription invokableDescription)
        {
            return AllDeclaredInvokables.First(it => it.Description.Id == invokableDescription.Id);
        }

        public void AddExternalReference(ExternalReference reference)
        {
            GdAssert.That(!_externalReferences.ContainsKey(reference.Id), "Tried to add an external reference twice.");
            _externalReferences[reference.Id] = reference;
        }


        public void RemoveExternalReference(ExternalReference externalReference)
        {
            var removed = _externalReferences.Remove(externalReference.Id);
            GdAssert.That(removed, "Tried to remove an external reference that was not present.");
        }


        public bool IsDefinedInThisProject(InvokableDescription invokableDescription)
        {
            switch (invokableDescription)
            {
                case FunctionDescription functionDescription:
                    return IsDefinedInThisProject(functionDescription);
                case ModuleDescription moduleDescription:
                    return IsDefinedInThisProject(moduleDescription);
                case MainModuleDescription mainModuleDescription:
                    return MainModule.Description.Id == mainModuleDescription.Id;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool TryGetExternalReferenceHolding(InvokableDescription description, out ExternalReference result)
        {
            foreach (var reference in _externalReferences.Values)
            {
                switch (description)
                {
                    case FunctionDescription _ when reference.Functions.Any(it => it.Id == description.Id):
                        result = reference;
                        return true;
                    case ModuleDescription _ when reference.Modules.Any(it => it.Id == description.Id):
                        result = reference;
                        return true;
                }
            }

            result = default;
            return false;
        }

        public bool TryGetExternalReferenceHolding(VariableDescription description, out ExternalReference result)
        {
            foreach (var externalReference in _externalReferences.Values
                         .Where(externalReference => externalReference.Variables.Any(it => it.Id == description.Id)))
            {
                result = externalReference;
                return true;
            }
            
            result = default;
            return false;
        }


        private bool IsDefinedInThisProject(FunctionDescription description)
        {
            return _projectFunctionDescriptions.ContainsKey(description.Id);
        }

        private bool IsDefinedInThisProject(ModuleDescription description)
        {
            return _projectModuleDescriptions.ContainsKey(description.Id);
        }

        public bool IsDefinedInThisProject(VariableDescription description)
        {
            return _projectVariables.ContainsKey(description.Id);
        }

    }
}