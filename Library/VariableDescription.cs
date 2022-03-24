using Godot;

namespace OpenScadGraphEditor.Library
{
    public class VariableDescription : Resource
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