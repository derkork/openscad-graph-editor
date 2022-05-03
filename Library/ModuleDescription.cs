using OpenScadGraphEditor.Library.IO;
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
        public bool SupportsChildren { get; set; }

        public override bool CanUse(ScadNode node)
        {
            return true;
        }

        public void LoadFrom(SavedModuleDescription savedInvokableDescription)
        {
            SupportsChildren = savedInvokableDescription.SupportsChildren;
            base.LoadFrom(savedInvokableDescription);
        }

        public void SaveInto(SavedModuleDescription savedInvokableDescription)
        {
            savedInvokableDescription.SupportsChildren = SupportsChildren;
            base.SaveInto(savedInvokableDescription);
        }
    }
}