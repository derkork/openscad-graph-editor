using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenScadGraphEditor.Library.External
{
    /// <summary>
    /// A reference to an external OpenSCAD file.
    /// </summary>
    public class ExternalReference : Resource, IReferenceResolver
    {
        /// <summary>
        /// Flag indicating whether the file has actually been loaded and the functions and modules and variables
        /// are up to date.
        /// </summary>
        [Export]
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Id of this external reference.
        /// </summary>
        [Export]
        public string Id { get; set; }
        
        /// <summary>
        /// The source file from which the declarations were loaded. This needs to be decoded with
        /// <see cref="PathResolver.Decode"/>. Use <see cref="IncludePath"/> accessor instead of this.
        /// </summary>
        [Export]
        public string SourceFile { get; set; }
        
        /// <summary>
        /// The include mode used to get this reference. (see https://en.wikibooks.org/wiki/OpenSCAD_User_Manual/Include_Statement).
        /// </summary>
        [Export]
        public IncludeMode Mode { get; set; }
        
        /// <summary>
        /// All functions defined in the source file.
        /// </summary>
        [Export]
        public List<FunctionDescription> Functions { get; set;  } = new List<FunctionDescription>();
        
        /// <summary>
        /// All modules defined in the source file.
        /// </summary>
        [Export]
        public List<ModuleDescription> Modules { get; set; } = new List<ModuleDescription>();
        
        /// <summary>
        /// All variables defined in the source file.
        /// </summary>
        [Export]
        public List<VariableDescription> Variables { get; set; } = new List<VariableDescription>();

        public FunctionDescription ResolveFunctionReference(string id)
        {
            return Functions.FirstOrDefault(f => f.Id == id);
        }

        public ModuleDescription ResolveModuleReference(string id)
        {
            return Modules.FirstOrDefault(m => m.Id == id);
        }

        public VariableDescription ResolveVariableReference(string id)
        {
            return Variables.FirstOrDefault(v => v.Id == id);
        }

        public ExternalReference ResolveExternalReference(string id)
        {
            return id == Id ? this : null;
        }

        /// <summary>
        /// Returns the include path as it will be printed into the scad file.
        /// </summary>
        public string IncludePath => PathResolver.Decode(SourceFile, out _);
    }
}