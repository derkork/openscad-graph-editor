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
        /// Customizer description.
        /// </summary>
        public VariableCustomizerDescription CustomizerDescription { get; set; } = new VariableCustomizerDescription();

        public void LoadFrom(SavedVariableDescription savedVariableDescription)
        {
            Id = savedVariableDescription.Id;
            Name = savedVariableDescription.Name;
            Description = savedVariableDescription.Description;
            TypeHint = savedVariableDescription.TypeHint;
            if (savedVariableDescription.CustomizerDescription != null)
            {
                CustomizerDescription.LoadFrom(savedVariableDescription.CustomizerDescription);
            }
            else
            {
                CustomizerDescription.Reset();
            }
        }

        public void SaveInto(SavedVariableDescription savedVariableDescription)
        {
            savedVariableDescription.Id = Id;
            savedVariableDescription.Name = Name;
            savedVariableDescription.Description = Description;
            savedVariableDescription.TypeHint = TypeHint;
            savedVariableDescription.CustomizerDescription = new SavedVariableCustomizerDescription();
            CustomizerDescription.SaveInto(savedVariableDescription.CustomizerDescription);
        }
    }
}