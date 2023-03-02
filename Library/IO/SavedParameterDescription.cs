using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// Saved variant of a <see cref="ParameterDescription"/>
    /// </summary>
    public sealed class SavedParameterDescription : Resource
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        [Export]
        public string Name { get; set; }

        /// <summary>
        /// The label of the parameter to be used when being rendered as a node.
        /// </summary>
        [Export]
        public string Label { get; set; } = "";

        /// <summary>
        /// The description of the parameter.
        /// </summary>
        [Export]
        public string Description { get; set; } = "";
        
        /// <summary>
        /// A type hint for the parameter.
        /// </summary>
        [Export]
        public PortType TypeHint { get; set; } = PortType.Any;

        /// <summary>
        /// A render hint for the parameter.
        /// </summary>
        [Export]
        public RenderHint RenderHint { get; set; } = RenderHint.None;
        
        /// <summary>
        /// Whether the parameter is optional.
        /// </summary>
        [Export]
        public bool IsOptional { get; set; }
        
        
        
        
 
    }
}