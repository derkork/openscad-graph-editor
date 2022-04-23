using System.Globalization;
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

        public string GetData(string key, string defaultValue = "")
        {
            return StoredData.TryGetValue(key, out var result) ? result : defaultValue;
        }

        public int GetDataInt(string key, int defaultValue = 0)
        {
            if (StoredData.TryGetValue(key, out var resultAsString) && int.TryParse(resultAsString, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        public double GetDataDouble(string key, double defaultValue = 0)
        {
            if (StoredData.TryGetValue(key, out var resultAsString) && double.TryParse(resultAsString, out var result))
            {
                return result;
            }
            return defaultValue;
        }


        public bool GetDataBool(string key, bool defaultValue = false)
        {
            if (StoredData.TryGetValue(key, out var resultAsString) && bool.TryParse(resultAsString, out var result))
            {
                return result;
            }

            return defaultValue;
        }
        
        
        public void SetData(string key, string value)
        {
            StoredData[key] = value;
        }

        public void SetData(string key, int value)
        {
            StoredData[key] = value.ToString();
        }

        public void SetData(string key, double value)
        {
            StoredData[key] = value.ToString(CultureInfo.InvariantCulture);
        }

        public void SetData(string key, bool value)
        {
            StoredData[key] = value.ToString();
        }
        
        
    }
}