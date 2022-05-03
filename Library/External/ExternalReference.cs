using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.Library.External
{
    /// <summary>
    /// A reference to an external OpenSCAD file.
    /// </summary>
    public class ExternalReference : ICanBeRendered
    {
        /// <summary>
        /// Flag indicating whether the file has actually been loaded and the functions and modules and variables
        /// are up to date.
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Flag marking whether this reference was made from the main project or from another library.
        /// </summary>
        public bool IsTransitive => !IncludedBy.Empty();

        /// <summary>
        /// ID of the external reference which has included this. Only defined if this is a transitive reference.
        /// </summary>
        public string IncludedBy { get; set; } = "";
        
        /// <summary>
        /// Paths included by this file (only _include_ but not _use_.).
        /// </summary>
        public List<string> References { get; set; } = new List<string>();

        /// <summary>
        /// Id of this external reference.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The include path that was used to get this reference.
        /// </summary>
        public string IncludePath { get; set; }
        
        /// <summary>
        /// The include mode used to get this reference. (see https://en.wikibooks.org/wiki/OpenSCAD_User_Manual/Include_Statement).
        /// </summary>
        public IncludeMode Mode { get; set; }
        
        /// <summary>
        /// All functions defined in the source file.
        /// </summary>
        public List<FunctionDescription> Functions { get; set;  } = new List<FunctionDescription>();
        
        /// <summary>
        /// All modules defined in the source file.
        /// </summary>
        public List<ModuleDescription> Modules { get; set; } = new List<ModuleDescription>();
        
        /// <summary>
        /// All variables defined in the source file.
        /// </summary>
        public List<VariableDescription> Variables { get; set; } = new List<VariableDescription>();


        public void LoadFrom(SavedExternalReference savedExternalReference)
        {
            Id = savedExternalReference.Id;
            IsLoaded = true;
            IncludedBy = savedExternalReference.IncludedBy;
            References = savedExternalReference.References.ToList();
            IncludePath = savedExternalReference.IncludePath;
            Mode = savedExternalReference.Mode;
            Functions = savedExternalReference.Functions
                .Select(it => (FunctionDescription) it.FromSavedState())
                .ToList();

            Modules = savedExternalReference.Modules
                .Select(it => (ModuleDescription) it.FromSavedState())
                .ToList();

            Variables = savedExternalReference.Variables
                .Select(it => it.FromSavedState())
                .ToList();
        }

        public void SaveInto(SavedExternalReference savedExternalReference)
        {
            savedExternalReference.Id = Id;
            savedExternalReference.IncludedBy = IncludedBy;
            savedExternalReference.References = new Array<string>(References);
            savedExternalReference.IncludePath = IncludePath;
            savedExternalReference.Mode = Mode;

            foreach (var functionDescription in Functions)
            {
                savedExternalReference.Functions.Add((SavedFunctionDescription) functionDescription.ToSavedState());
            }
            
            foreach (var moduleDescription in Modules)
            {
                savedExternalReference.Modules.Add((SavedModuleDescription) moduleDescription.ToSavedState());
            }
            
            savedExternalReference.Variables = new Array<SavedVariableDescription>();
            foreach (var variableDescription in Variables)
            {
                savedExternalReference.Variables.Add(variableDescription.ToSavedState());
            }
        }
        
        

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