using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace OpenScadGraphEditor.Library.External
{
    /// <summary>
    /// A reference to an external OpenSCAD file.
    /// </summary>
    public class ExternalReference : Resource, ICanBeRendered
    {
        /// <summary>
        /// Flag indicating whether the file has actually been loaded and the functions and modules and variables
        /// are up to date.
        /// </summary>
        [Export]
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Flag marking whether this reference was made from the main project or from another library.
        /// </summary>
        public bool IsTransitive => !IncludedBy.Empty();

        /// <summary>
        /// ID of the external reference which has included this. Only defined if this is a transitive reference.
        /// </summary>
        [Export]
        public string IncludedBy { get; set; } = "";
        
        /// <summary>
        /// Paths included by this file (only _include_ but not _use_.).
        /// </summary>
        [Export]
        public List<string> References { get; set; } = new List<string>();

        /// <summary>
        /// Id of this external reference.
        /// </summary>
        [Export]
        public string Id { get; set; }
        
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


        public string Render()
        {
            if (IsTransitive)
            {
                return "";
            }
            
            switch (Mode)
            {
                case IncludeMode.Include:
                    return $"include <{IncludePath}>;\n";
                case IncludeMode.Use:
                    return $"use <{IncludePath}>;\n";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}