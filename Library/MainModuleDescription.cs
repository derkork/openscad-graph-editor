using System;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// Description of the main module.
    /// </summary>
    public class MainModuleDescription : InvokableDescription
    {
        public MainModuleDescription()
        {
            Id = Guid.NewGuid().ToString();
            Name = "<main>";
            Description = "The main entrypoint.";
            IsExternal = false;
            IsBuiltin = true;
        }

        public override bool CanUse(ScadNode node)
        {
            return true;
        }
    }
}