using System;

namespace OpenScadGraphEditor.Library
{
    public static class VariableBuilder
    {
        public static VariableDescription NewVariable(string name, string id = "")
        {
            var result = new VariableDescription
            {
                Id = id.Length > 0 ? id : Guid.NewGuid().ToString(),
                Name = name
            };
            return result;
        }
    }
}