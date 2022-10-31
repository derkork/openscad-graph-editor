using System.Collections.Generic;
using Godot;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// Base class for customizer descriptions.
    /// </summary>
    public class SavedVariableCustomizerDescription : Resource
    {
     
        /// <summary>
        /// The constraint type that is applied on the variable in the customizer.
        /// </summary>
        [Export]
        public VariableCustomizerConstraintType ConstraintType { get; set; } = VariableCustomizerConstraintType.None;

        /// <summary>
        /// The name of the tab in which the variable is displayed. If empty, the variable is displayed in the default tab.
        /// </summary>
        [Export]
        public string Tab { get; set; } = "";
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.MinStepMax"/>, this property contains
        /// the minimum value of the variable.
        /// </summary>
        [Export]
        public string Min { get; set; } = "";
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.MinStepMax"/>, this property contains
        /// the step value of the variable.
        /// </summary>
        [Export]
        public string Step { get; set; } = "";
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.MinStepMax"/>, this property contains
        /// the maximum value of the variable.
        /// </summary>
        [Export]
        public string Max { get; set; } = "";
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.MaxLength"/>, this property contains
        /// the maximum length of the string.
        /// </summary>
        [Export]
        public string MaxLength { get; set; } = "";

        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.Options"/>, this property contains
        /// the serialized values of the options.
        /// </summary>
        [Export]
        public List<string> OptionValues { get; set; } = new List<string>();
        
        /// <summary>
        /// When the constraint type is <see cref="VariableCustomizerConstraintType.Options"/>, this property contains
        /// the labels of the options.
        /// </summary>
        [Export]
        public List<string> OptionLabels { get; set; } = new List<string>();

    }
}