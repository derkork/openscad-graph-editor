using System;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Library
{
    public static class VariableBuilder
    {
        public static VariableDescription NewVariable(string name, string id = "")
        {
            var result = Prefabs.New<VariableDescription>();
            result.Id = id.Length > 0 ? id : Guid.NewGuid().ToString();
            result.Name = name;
            return result;
        }
    }
}