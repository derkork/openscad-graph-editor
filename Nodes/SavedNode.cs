using Godot;
using Godot.Collections;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// This resource represents a saved graph
    /// </summary>
    public class SavedNode : Resource
    {
        [Export]
        public string Script;

        [Export]
        public Vector2 Position;

        [Export]
        public string Id;

        [Export]
        public Dictionary<string, string> StoredData = new Dictionary<string, string>();

        public string GetData(string key)
        {
            if (StoredData.TryGetValue(key, out var result))
            {
                return result;
            }

            return "";
        }

        public void SetData(string key, string value)
        {
            StoredData[key] = value;
        }
    }
}