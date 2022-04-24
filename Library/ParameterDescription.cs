using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// A description of a parameter of an Invokable.
    /// </summary>
    public sealed class ParameterDescription : Resource
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
        /// Returns the label of the parameter if set, otherwise it's name.
        /// </summary>
        public string LabelOrFallback => Label.Length > 0 ? Label : Name;
    }
}