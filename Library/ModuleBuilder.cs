using System;
using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    public readonly struct ModuleBuilder
    {
        private readonly ModuleDescription _currentModuleDescription;

        private ModuleBuilder(ModuleDescription currentModuleDescription)
        {
            _currentModuleDescription = currentModuleDescription;
        }

        public static ModuleBuilder NewModule(string name, string id = "")
        {
            var builder = new ModuleBuilder(Prefabs.New<ModuleDescription>())
            {
                _currentModuleDescription =
                {
                    Name = name,
                    Id = id.Length > 0 ? id : Guid.NewGuid().ToString()
                }
            };
            return builder;
        }

        public static ModuleBuilder NewBuiltInModule(string name, string nodeName = "", string idSuffix = "")
        {
            var builder = NewModule(name, "__builtin__module__" + name + idSuffix);
            builder._currentModuleDescription.IsBuiltin = true;
            builder._currentModuleDescription.NodeName = nodeName;
            return builder;
        }

        public ModuleBuilder WithDescription(string description)
        {
            _currentModuleDescription.Description = description;
            return this;
        }

        public ModuleBuilder WithChildren(bool value = true)
        {
            _currentModuleDescription.SupportsChildren = value;
            return this;
        }

        public ModuleBuilder WithParameter(string name, PortType typeHint = PortType.Any,
            string label = "", string description = "", bool optional = false)
        {
            GdAssert.That(_currentModuleDescription.Parameters.All(it => it.Name != name), $"Parameter with name '{name}' already exists");
            
            var parameter = Prefabs.New<ParameterDescription>();
            parameter.Name = name;
            parameter.Description = description;
            parameter.TypeHint = typeHint;
            parameter.Label = label;
            parameter.IsOptional = optional;

            _currentModuleDescription.Parameters.Add(parameter);
            return this;
        }

        public ModuleBuilder WithFragmentParameters()
        {
            return WithParameter("$fa", PortType.Number, label: "Minimum angle", optional: true)
                .WithParameter("$fs", PortType.Number, label: "Minimum length", optional: true)
                .WithParameter("$fn", PortType.Number, label: "Minimum # segments", optional: true);
        }

        public ModuleDescription Build()
        {
            return _currentModuleDescription;
        }
    }
}