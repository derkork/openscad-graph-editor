using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// A description of a parameter of an Invokable.
    /// </summary>
    public sealed class ParameterDescription 
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The label of the parameter to be used when being rendered as a node.
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// The description of the parameter.
        /// </summary>
        public string Description { get; set; } = "";
        
        /// <summary>
        /// A type hint for the parameter.
        /// </summary>
        public PortType TypeHint { get; set; } = PortType.Any;

        /// <summary>
        /// Whether the parameter is optional.
        /// </summary>
        public bool IsOptional { get; set; }
   
        
        /// <summary>
        /// Returns the label of the parameter if set, otherwise it's name.
        /// </summary>
        public string LabelOrFallback => Label.Length > 0 ? Label : Name;

        public void LoadFrom(SavedParameterDescription parameter)
        {
            Name = parameter.Name;
            Label = parameter.Label;
            Description = parameter.Description;
            TypeHint = parameter.TypeHint;
            IsOptional = parameter.IsOptional;
        }

        public void SaveInto(SavedParameterDescription parameter)
        {
            parameter.Name = Name;
            parameter.Label = Label;
            parameter.Description = Description;
            parameter.TypeHint = TypeHint;
            parameter.IsOptional = IsOptional;
        }
    }
}