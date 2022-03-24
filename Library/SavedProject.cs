using Godot;
using Godot.Collections;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// A saved project.
    /// </summary>
    public class SavedProject : Resource
    {
        /// <summary>
        /// The functions defined in the project
        /// </summary>
        [Export] public Array<SavedGraph> Functions = new Array<SavedGraph>();

        /// <summary>
        /// The modules defined in the project
        /// </summary>
        [Export] public Array<SavedGraph> Modules = new Array<SavedGraph>();

        /// <summary>
        /// The main module.
        /// </summary>
        [Export] public SavedGraph MainModule;

        /// <summary>
        /// All defined variables.
        /// </summary>
        [Export] public Array<VariableDescription> Variables = new Array<VariableDescription>();

    }
}