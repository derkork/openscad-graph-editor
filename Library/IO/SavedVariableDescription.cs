using Godot;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// Saved variant of <see cref="VariableDescription"/>
    /// </summary>
    public class SavedVariableDescription : Resource
    {
        /// <summary>
        /// Id of the variable.
        /// </summary>
        [Export]
        public string Id { get; set; } = "";

        /// <summary>
        /// The name of the variable.
        /// </summary>
        [Export]
        public string Name { get; set; } = "";
        
    }
}