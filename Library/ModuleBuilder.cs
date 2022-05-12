using System;
using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Nodes;

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
            var builder = new ModuleBuilder(new ModuleDescription())
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

            var parameter = new ParameterDescription
            {
                Name = name,
                Description = description,
                TypeHint = typeHint,
                Label = label,
                IsOptional = optional
            };

            _currentModuleDescription.Parameters.Add(parameter);
            return this;
        }

        public ModuleBuilder WithFragmentParameters()
        {
            return WithParameter("$fa", PortType.Number, label: "Minimum angle", optional: true, description: "The minimum angle between fragments")
                .WithParameter("$fs", PortType.Number, label: "Minimum length", optional: true, description: "The minimum length of a fragment")
                .WithParameter("$fn", PortType.Number, label: "Minimum # segments", optional: true, description: "The minimum number of segments that should be created.");
        }

        public ModuleDescription Build()
        {
            return _currentModuleDescription;
        }
    }
}