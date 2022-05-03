using Godot;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// Saved variant of a <see cref="ModuleDescription"/>
    /// </summary>
    public sealed class SavedModuleDescription : SavedInvokableDescription
    {
        /// <summary>
        /// Whether or not this module supports children 
        /// </summary>
        [Export]
        public bool SupportsChildren { get; set; }
    }
}