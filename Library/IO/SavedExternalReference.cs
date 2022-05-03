using Godot;
using Godot.Collections;
using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Library.IO
{
    public class SavedExternalReference : Resource
    {
        /// <summary>
        /// Id of this external reference.
        /// </summary>
        [Export]
        public string Id { get; set; }

        /// <summary>
        /// ID of the external reference which has included this. Only defined if this is a transitive reference.
        /// </summary>
        [Export]
        public string IncludedBy { get; set; } = "";
        
        /// <summary>
        /// Paths included by this file (only _include_ but not _use_.).
        /// </summary>
        [Export]
        public Array<string> References { get; set; } = new Array<string>();

        
        /// <summary>
        /// The include path that was used to get this reference.
        /// </summary>
        [Export]
        public string IncludePath { get; set; }
        
        /// <summary>
        /// The include mode used to get this reference. (see https://en.wikibooks.org/wiki/OpenSCAD_User_Manual/Include_Statement).
        /// </summary>
        [Export]
        public IncludeMode Mode { get; set; }
        
        /// <summary>
        /// All functions defined in the source file.
        /// </summary>
        [Export]
        public Array<SavedFunctionDescription> Functions { get; set;  } = new Array<SavedFunctionDescription>();
        
        /// <summary>
        /// All modules defined in the source file.
        /// </summary>
        [Export]
        public Array<SavedModuleDescription> Modules { get; set; } = new Array<SavedModuleDescription>();
        
        /// <summary>
        /// All variables defined in the source file.
        /// </summary>
        [Export]
        public Array<SavedVariableDescription> Variables { get; set; } = new Array<SavedVariableDescription>();


    }
}