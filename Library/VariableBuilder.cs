using System;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    public static class VariableBuilder
    {
        public static VariableDescription NewVariable(string name)
        {
            var result = Prefabs.New<VariableDescription>();
            result.Id = Guid.NewGuid().ToString();
            result.Name = name;
            return result;
        }
    }
}