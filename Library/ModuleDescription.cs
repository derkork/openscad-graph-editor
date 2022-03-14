using System.Collections.Generic;
using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// This describes a module that can be invoked.
    /// </summary>
    public class ModuleDescription : InvokableDescription
    {
        /// <summary>
        /// Whether or not this module supports children 
        /// </summary>
        [Export]
        public bool SupportsChildren { get; set; }

        public override bool CanUse(ScadNode node)
        {
            return true;
        }
    }
}