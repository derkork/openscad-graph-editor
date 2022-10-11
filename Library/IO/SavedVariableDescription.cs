using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// Saved variant of <see cref="VariableDescription"/>
    /// </summary>
    public class SavedVariableDescription : Resource
    {
        /// <summary>
        /// Id of the variable.
        /// </summary>
        [Export]
        public string Id { get; set; } = "";

        /// <summary>
        /// The name of the variable.
        /// </summary>
        [Export]
        public string Name { get; set; } = "";

        
        /// <summary>
        /// Description of the variable.
        /// </summary>
        [Export]
        public string Description { get; set; } = "";


        /// <summary>
        /// Type hint for the variable.
        /// </summary>
        [Export]
        public PortType TypeHint { get; set; } = PortType.Any;

        /// <summary>
        /// The default value of the variable, or null if it is not set.
        /// </summary>
        [Export]
        [CanBeNull]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Should the variable be shown in the customizer?
        /// </summary>
        [Export]
        public bool ShowInCustomizer { get; set; } = true;
        
        /// <summary>
        /// Customizer setup for this variable.
        /// </summary>
        [Export]
        [CanBeNull]
        public SavedVariableCustomizerDescription CustomizerDescription { get; set; }
    }
}