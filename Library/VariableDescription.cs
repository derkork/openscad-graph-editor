using JetBrains.Annotations;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Library
{
    public class VariableDescription
    {
        /// <summary>
        /// Id of the variable.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// The name of the variable.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// The description of the variable.
        /// </summary>
        public string Description { get; set; } = "";
        
        /// <summary>
        /// The type of the variable.
        /// </summary>
        public PortType TypeHint { get; set; } = PortType.Any;

        /// <summary>
        /// Should the variable appear in the customizer?
        /// </summary>
        public bool ShowInCustomizer { get; set; } = true;
        
        /// <summary>
        /// Customizer description.
        /// </summary>
        public VariableCustomizerDescription CustomizerDescription { get; set; } = new VariableCustomizerDescription();

        
        /// <summary>
        /// The default value of this variable or null if there is no default value.
        /// </summary>
        [CanBeNull]
        public IScadLiteral DefaultValue { get; set; }
        
        public void LoadFrom(SavedVariableDescription savedVariableDescription)
        {
            Id = savedVariableDescription.Id;
            Name = savedVariableDescription.Name;
            Description = savedVariableDescription.Description;
            TypeHint = savedVariableDescription.TypeHint;
            ShowInCustomizer = savedVariableDescription.ShowInCustomizer;
            
            if (savedVariableDescription.DefaultValue != null)
            {
                // we build a new literal based on the variable type.
                var literalType = TypeHint.GetMatchingLiteralType();
                DefaultValue = literalType.BuildLiteral(savedVariableDescription.DefaultValue);
            }
            
            
            if (savedVariableDescription.CustomizerDescription != null)
            {
                CustomizerDescription.LoadFrom(this, savedVariableDescription.CustomizerDescription);
            }
            else
            {
                CustomizerDescription = new VariableCustomizerDescription();
            }
        }

        public void SaveInto(SavedVariableDescription savedVariableDescription)
        {
            savedVariableDescription.Id = Id;
            savedVariableDescription.Name = Name;
            savedVariableDescription.Description = Description;
            savedVariableDescription.TypeHint = TypeHint;
            savedVariableDescription.DefaultValue = DefaultValue?.SerializedValue;
            savedVariableDescription.ShowInCustomizer = ShowInCustomizer;
            savedVariableDescription.CustomizerDescription = new SavedVariableCustomizerDescription();
            CustomizerDescription.SaveInto(savedVariableDescription.CustomizerDescription);
        }
    }
}