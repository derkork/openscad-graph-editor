using System.Collections.Generic;
using Godot;
using Serilog;

namespace OpenScadGraphEditor
{
    public class Configuration
    {
        private const string ConfigFolder = "user://openscad_graph_editor";
        private const string ConfigPath = ConfigFolder+"/openscad_graph_editor.cfg";
        private ConfigFile _configFile = new ConfigFile();


        public void Load()
        {
            if (_configFile.Load(ConfigPath) != Error.Ok)
            {
                Log.Information("Cannot read config file, starting with clean slate");
                _configFile.Clear();
            }
        }

        private void Save()
        {
            var directory = new Directory();
            directory.MakeDirRecursive(ConfigFolder);

            var error = _configFile.Save(ConfigPath);
            if (error != Error.Ok)
            {
                Log.Warning("Couldn't save config file to {Path} -> Error {Error}", ConfigPath, error);
            }
        }

        public void AddRecentFile(string filePath)
        {
            var recentFiles = GetRecentFiles();
            var indexOf = recentFiles.IndexOf(filePath);
            if (indexOf != -1)
            {
                // swap with first
                recentFiles[indexOf] = recentFiles[0];
                recentFiles[0] = filePath;
            }
            else
            {
                // add to beginning
                recentFiles.Insert(0, filePath);
                
                // remove last if too many
                if (recentFiles.Count > 10)
                {
                    recentFiles.RemoveAt(10);
                }
            }
            
            // now serialize this back into the config file
            _configFile.SetValue("recent_files", "count", recentFiles.Count);
            for (var i = 0; i < recentFiles.Count; i++)
            {
                _configFile.SetValue("recent_files", $"file_{i}", recentFiles[i]);
            }

            Save();
        }

        public List<string> GetRecentFiles()
        {
            var result = new List<string>();
            var count = (int) _configFile.GetValue("recent_files", "count", 0);
            for (var i = 0; i < count; i++)
            {
                var key = $"file_{i}";
                var value = (string) _configFile.GetValue("recent_files", key, "");
                if (!value.Empty())
                {
                    result.Add(value);
                }
            }

            return result;
        }
    }
}