using Godot;
using Godot.Collections;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// Saved variant of a <see cref="InvokableDescription"/>
    /// </summary>
    public abstract class SavedInvokableDescription : Resource
    {
        /// <summary>
        /// Id of the description.
        /// </summary>
        [Export]
        public string Id { get; set; } = "";

        /// <summary>
        /// The name of the invokable (e.g. function/module name).
        /// </summary>
        [Export]
        public string Name { get; set; } = "";

        /// <summary>
        /// The name that the node representing this should have. If not set, the <see cref="Name"/> will be used to
        /// name the node.
        /// </summary>
        [Export]
        public string NodeName { get; set; } = "";

        /// <summary>
        /// A description of the invokable.
        /// </summary>
        [Export]
        public string Description { get; set; } = "";

        /// <summary>
        /// Whether this description originated from an external source.
        /// </summary>
        [Export]
        public bool IsExternal { get; set; }

        /// <summary>
        /// Whether this description is for a built-in invokable.
        /// </summary>
        [Export]
        public bool IsBuiltin { get; set; }

        /// <summary>
        /// Path to the external source file where this description originated.
        /// </summary>
        [Export]
        public string ExternalSource { get; set; } = "";

        /// <summary>
        /// The parameters of the function/module.
        /// </summary>
        [Export]
        public Array<SavedParameterDescription> Parameters { get; set; } = new Array<SavedParameterDescription>();
    }
}