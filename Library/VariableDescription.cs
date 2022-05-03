using OpenScadGraphEditor.Library.IO;

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

        public void LoadFrom(SavedVariableDescription savedVariableDescription)
        {
            Id = savedVariableDescription.Id;
            Name = savedVariableDescription.Name;
        }

        public void SaveInto(SavedVariableDescription savedVariableDescription)
        {
            savedVariableDescription.Id = Id;
            savedVariableDescription.Name = Name;
        }
    }
}