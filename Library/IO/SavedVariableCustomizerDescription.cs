using Godot;
using Godot.Collections;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// Base class for customizer descriptions.
    /// </summary>
    public class SavedVariableCustomizerDescription : Resource
    {
     
        [Export]
        public Dictionary<string, object> StoredData = new Dictionary<string, object>();
    }
}