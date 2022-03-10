using Godot;
using Godot.Collections;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// This resource represents a saved graph
    /// </summary>
    public class SavedNode : Resource
    {
        [Export]
        public string Type;

        [Export]
        public Vector2 Position;

        [Export]
        public string Id;

        [Export]
        public Dictionary<string, string> StoredData = new Dictionary<string, string>();

        public string GetData(string key)
        {
            return StoredData.TryGetValue(key, out var result) ? result : "";
        }

        public void SetData(string key, string value)
        {
            StoredData[key] = value;
        }
    }
}