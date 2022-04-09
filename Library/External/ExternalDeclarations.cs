using System.Collections.Generic;
using Godot;

namespace OpenScadGraphEditor.Library.External
{
    public class ExternalDeclarations : Resource
    {
        /// <summary>
        /// The source file from which the declarations were loaded.
        /// </summary>
        [Export]
        public string SourceFile { get; set; }
        
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
    }
}