using Godot;
using Godot.Collections;

namespace OpenScadGraphEditor.Library.IO
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
        [Export] public Array<SavedVariableDescription> Variables = new Array<SavedVariableDescription>();
        
                
        /// <summary>
        /// All references to external files.
        /// </summary>
        [Export]
        public Array<SavedExternalReference> ExternalReferences = new Array<SavedExternalReference>();

    }
}