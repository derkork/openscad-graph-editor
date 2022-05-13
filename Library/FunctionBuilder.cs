using System;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    public readonly struct FunctionBuilder
    {
        private readonly FunctionDescription _currentFunctionDescription;

        private FunctionBuilder(FunctionDescription currentFunctionDescription)
        {
            _currentFunctionDescription = currentFunctionDescription;
        }

        public static FunctionBuilder NewFunction(string name, string id = "", PortType returnType = PortType.Any, string returnValueDescription = "")
        {
            var builder = new FunctionBuilder(new FunctionDescription())
            {
                _currentFunctionDescription =
                {
                    Name = name,
                    ReturnTypeHint = returnType,
                    Id = id.Length > 0 ? id : Guid.NewGuid().ToString(),
                    ReturnValueDescription = returnValueDescription
                }
            };
            return builder;
        }

        public static FunctionBuilder NewBuiltInFunction(string name, string nodeName = "",
            PortType returnType = PortType.Any, string idSuffix = "", string returnValueDescription = "")
        {
            var builder = NewFunction(name, "__builtin__function__" + name + idSuffix, returnType, returnValueDescription);
            builder._currentFunctionDescription.IsBuiltin = true;
            builder._currentFunctionDescription.NodeName = nodeName;
            return builder;
        }

        public FunctionBuilder WithDescription(string description)
        {
            _currentFunctionDescription.Description = description;
            return this;
        }

        public FunctionBuilder WithParameter(string name, PortType typeHint = PortType.Any,
            string label = "", string description = "", bool optional = false)
        {
            var parameter = new ParameterDescription
            {
                Name = name,
                Description = description,
                TypeHint = typeHint,
                Label = label,
                IsOptional = optional
            };

            _currentFunctionDescription.Parameters.Add(parameter);
            return this;
        }

        public FunctionDescription Build()
        {
            return _currentFunctionDescription;
        }
    }
}