using System;
using System.Collections.Generic;
using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    public class ScadProject : ICanBeRendered, IReferenceResolver
    {
        private readonly IReferenceResolver _parentResolver;

        private readonly Dictionary<string, FunctionDescription> _projectFunctionDescriptions =
            new Dictionary<string, FunctionDescription>();

        private readonly Dictionary<string, ModuleDescription> _projectModuleDescriptions =
            new Dictionary<string, ModuleDescription>();

        private readonly Dictionary<string, VariableDescription> _projectVariables =
            new Dictionary<string, VariableDescription>();

        private readonly Dictionary<string, ExternalReference> _externalReferences = new Dictionary<string, ExternalReference>();

        private readonly HashSet<IScadGraph> _modules = new HashSet<IScadGraph>();
        private readonly HashSet<IScadGraph> _functions = new HashSet<IScadGraph>();

        public IEnumerable<IScadGraph> Modules => _modules.OrderBy(x => x.Description.Name);
        public IEnumerable<IScadGraph> Functions => _functions.OrderBy(x => x.Description.Name);

        public IEnumerable<IScadGraph> AllDeclaredInvokables => Functions.Concat(Modules).Append(MainModule);

        public IEnumerable<VariableDescription> Variables => _projectVariables.Values.OrderBy(x => x.Name);
        public IEnumerable<ExternalReference> ExternalReferences => _externalReferences.Values.OrderBy(x => x.IncludePath);

        public IScadGraph MainModule { get; private set; }

        public string ProjectPath { get; private set; }

        public ScadProject(IReferenceResolver parentResolver)
        {
            _parentResolver = parentResolver;
            var mainModuleGraph = new LightWeightGraph();
            mainModuleGraph.Main();
            MainModule = mainModuleGraph;
        }


        public void TransferData(IScadGraph from, IScadGraph to)
        {
            GdAssert.That(from == MainModule || _functions.Contains(from) || _modules.Contains(from),
                "'from' graph is not part of this project.");

            var savedGraph = Prefabs.New<SavedGraph>();
            from.SaveInto(savedGraph);
            to.LoadFrom(savedGraph, this);

            switch (to.Description)
            {
                case MainModuleDescription _:
                    MainModule = to;
                    break;
                case FunctionDescription _:
                    _functions.Remove(from);
                    _functions.Add(to);
                    break;
                case ModuleDescription _:
                    _modules.Remove(from);
                    _modules.Add(to);
                    break;
                default:
                    throw new InvalidOperationException("unknown description type.");
            }
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
            _modules.ForAll(it => it.Discard());
            _functions.ForAll(it => it.Discard());

            MainModule.Discard();
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
                _projectFunctionDescriptions[function.Description.Id] = (FunctionDescription) function.Description;
            }

            foreach (var module in project.Modules)
            {
                _projectModuleDescriptions[module.Description.Id] = (ModuleDescription) module.Description;
            }

            // Step 2: load variable descriptions so we can resolve them in step 4
            foreach (var variable in project.Variables)
            {
                _projectVariables.Add(variable.Id, variable);
            }

            // Step 3: load external references so we can resolve them in step 4
            foreach (var external in project.ExternalReferences)
            {
                _externalReferences.Add(external.Id, external);
            }

            // Step 4: load the actual graphs, which can now resolve references to other functions.
            foreach (var function in project.Functions)
            {
                var functionContext = new LightWeightGraph();
                functionContext.LoadFrom(function, this);
                _functions.Add(functionContext);
            }

            foreach (var module in project.Modules)
            {
                var moduleContext = new LightWeightGraph();
                moduleContext.LoadFrom(module, this);
                _modules.Add(moduleContext);
            }

            MainModule = new LightWeightGraph();
            MainModule.LoadFrom(project.MainModule, this);
        }

        public SavedProject Save()
        {
            var result = Prefabs.New<SavedProject>();

            _externalReferences.ForAll(it => result.ExternalReferences.Add(it.Value));

            foreach (var function in _functions)
            {
                var savedGraph = Prefabs.New<SavedGraph>();
                function.SaveInto(savedGraph);
                result.Functions.Add(savedGraph);
            }

            foreach (var module in _modules)
            {
                var savedGraph = Prefabs.New<SavedGraph>();
                module.SaveInto(savedGraph);
                result.Modules.Add(savedGraph);
            }

            foreach (var variable in _projectVariables.Values)
            {
                result.Variables.Add(variable);
            }

            {
                var savedGraph = Prefabs.New<SavedGraph>();
                MainModule.SaveInto(savedGraph);
                result.MainModule = savedGraph;
            }

            return result;
        }

        public string Render()
        {
            return string.Join("\n",
                _externalReferences.Select(it => it.Value.Render())
                    .Concat(_modules.Select(it => it.Render()))
                    .Concat(_functions.Select(it => it.Render()))
                    .Append(MainModule.Render())
                    .Where(it => it.Length > 0)
            );
        }

        public void Discard()
        {
            _modules.ForAll(it => it.Discard());
            _functions.ForAll(it => it.Discard());
            MainModule?.Discard();
        }

        public IScadGraph AddInvokable(InvokableDescription invokableDescription)
        {
            var graph = new LightWeightGraph();
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
                    graph.Discard();
                    break;
                case ModuleDescription _:
                    _modules.Remove(graph);
                    _projectModuleDescriptions.Remove(description.Id);
                    graph.Discard();
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

        public IScadGraph FindDefiningGraph(InvokableDescription invokableDescription)
        {
            return AllDeclaredInvokables.First(it => it.Description == invokableDescription);
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
        
        
        public bool IsDefinedInThisProject(FunctionDescription description)
        {
            return _projectFunctionDescriptions.ContainsKey(description.Id);
        }

        public bool IsDefinedInThisProject(ModuleDescription description)
        {
            return _projectModuleDescriptions.ContainsKey(description.Id);
        }

        public bool IsDefinedInThisProject(VariableDescription description)
        {
            return _projectVariables.ContainsKey(description.Id);
        }

    }
}