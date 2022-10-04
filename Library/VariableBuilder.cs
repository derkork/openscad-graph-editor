using System;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public class VariableBuilder
    {
        private VariableDescription _result;
        
        private VariableBuilder() {}
        
        public static VariableBuilder NewVariable(string name, string id = "")
        {
            var result = new VariableBuilder
            {
                _result = new VariableDescription
                {
                    Id = id.Length > 0 ? id : Guid.NewGuid().ToString(),
                    Name = name
                }
            };
            return result;
        }
        
        // description
        public VariableBuilder WithDescription(string description)
        {
            _result.Description = description;
            return this;
        }
        
        // type
        public VariableBuilder WithType(PortType type)
        {
            _result.TypeHint = type;
            return this;
        }
        
        // build
        public VariableDescription Build()
        {
            return _result;
        }
    }
}