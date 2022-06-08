using System;
using Godot;
using Newtonsoft.Json;

namespace GodotXUnitApi.Internal
{
    public static class WorkFiles
    {
        public static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public const string WorkDir = "res://addons/GodotXUnit/_work";

        public static string PathForResFile(string filename)
        {
            var appending = filename.EndsWith(".json") ? filename : $"{filename}.json";
            return $"{WorkDir}/{appending}";
        }

        public static void CleanWorkDir()
        {
            var directory = new Godot.Directory();
            directory.MakeDirRecursive(WorkDir).ThrowIfNotOk();
            directory.Open(WorkDir).ThrowIfNotOk();
            directory.ListDirBegin(true, true).ThrowIfNotOk();
            while (true)
            {
                var next = directory.GetNext();
                if (string.IsNullOrEmpty(next))
                    break;
                directory.Remove(next).ThrowIfNotOk();
            }
            directory.ListDirEnd();
        }

        public static void WriteFile(string filename, object contents)
        {
            var writing = JsonConvert.SerializeObject(contents, Formatting.Indented, jsonSettings);
            var file = new Godot.File();
            file.Open(PathForResFile(filename), File.ModeFlags.WriteRead).ThrowIfNotOk();
            try
            {
                file.StoreString(writing);
            }
            finally
            {
                file.Close();
            }
        }

        public static object ReadFile(string filename)
        {
            var file = new Godot.File();
            file.Open(PathForResFile(filename), File.ModeFlags.Read).ThrowIfNotOk();
            try
            {
                return JsonConvert.DeserializeObject(file.GetAsText(), jsonSettings);
            }
            finally
            {
                file.Close();
            }
        }
    }
}