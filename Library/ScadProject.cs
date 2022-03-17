using System;
using System.Collections.Generic;
using System.Linq;
using GodotExt;
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

        private readonly HashSet<IScadGraph> _modules = new HashSet<IScadGraph>();
        private readonly HashSet<IScadGraph> _functions = new HashSet<IScadGraph>();

        public IEnumerable<IScadGraph> Modules => _modules.OrderBy(x => x.Description.Name);
        public IEnumerable<IScadGraph> Functions => _functions.OrderBy(x => x.Description.Name);

        public IScadGraph MainModule { get; private set; }

        public ScadProject(IReferenceResolver parentResolver)
        {
            _parentResolver = parentResolver;
            var mainModuleGraph = new LightWeightGraph();
            mainModuleGraph.Main();
            MainModule = mainModuleGraph;
        }


        public void TransferData(IScadGraph from, IScadGraph to)
        {
            GdAssert.That(from == MainModule || _functions.Contains(from) || _modules.Contains(from), "'from' graph is not part of this project.");
            
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
            
            from.Discard();
        }
        
        public FunctionDescription ResolveFunctionReference(string id)
        {
            return _projectFunctionDescriptions.TryGetValue(id, out var result)
                ? result
                : _parentResolver.ResolveFunctionReference(id);
        }

        public ModuleDescription ResolveModuleReference(string id)
        {
            return _projectModuleDescriptions.TryGetValue(id, out var result)
                ? result
                : _parentResolver.ResolveModuleReference(id);
        }

        private void Clear()
        {
            _projectFunctionDescriptions.Clear();
            _projectModuleDescriptions.Clear();
            _modules.ForAll(it => it.Discard());
            _functions.ForAll(it => it.Discard());
            MainModule.Discard();

            _modules.Clear();
            _functions.Clear();
        }

        public void Load(SavedProject project)
        {
            Clear();
            // Step 1: load function descriptions so we can resolve them in step 2
            foreach (var function in project.Functions)
            {
                _projectFunctionDescriptions[function.Description.Id] = (FunctionDescription) function.Description;
            }
            foreach (var module in project.Modules)
            {
                _projectModuleDescriptions[module.Description.Id] = (ModuleDescription) module.Description;
            }
            
            // Step 2: load the actual graphs, which can now resolve references to other functions.
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
                _modules.Select(it => it.Render())
                    .Union(_functions.Select(it => it.Render()))
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
    }
}