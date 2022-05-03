using System;
using OpenScadGraphEditor.Library.IO;
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
            return !(node is Child || node is Children);
        }
        
        public void LoadFrom(SavedMainModuleDescription savedInvokableDescription)
        {
            base.LoadFrom(savedInvokableDescription);
        }

        public void SaveInto(SavedMainModuleDescription savedInvokableDescription)
        {
            base.SaveInto(savedInvokableDescription);
        }
    }
}