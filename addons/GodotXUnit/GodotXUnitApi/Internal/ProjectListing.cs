using System;
using System.Collections.Generic;
using System.IO;
using GodotGD = Godot.GD;

namespace GodotXUnitApi.Internal
{
    public static class ProjectListing
    {
        public static readonly string sep = Path.DirectorySeparatorChar.ToString();
        
        private static string _projectDir;
        
        public static string ProjectDir
        {
            get
            {
                if (!string.IsNullOrEmpty(_projectDir))
                    return _projectDir;
                var current = Directory.GetCurrentDirectory();
                while (!string.IsNullOrEmpty(current))
                {
                    if (File.Exists($"{current}{sep}project.godot"))
                    {
                        _projectDir = current;
                        return _projectDir;
                    }
                    current = Directory.GetParent(current).FullName;
                }
                GodotGD.PrintErr("unable to find root of godot project");
                throw new Exception("unable to find root dir");
                
                // TODO: if this becomes a problem, we can do OS.Execute('pwd'....), but i don't
                // want to do that if we don't need to.
            }
        }
        
        public static List<string> GetProjectList()
        {
            var result = new List<string>();
            foreach (var filename in Directory.GetFiles(ProjectDir, "*.csproj", SearchOption.AllDirectories))
            {
                if (filename.Contains("GodotXUnitApi"))
                    continue;
                result.Add(Path.GetFileNameWithoutExtension(filename));
            }
            return result;
        }
        
        public static Dictionary<string, string> GetProjectInfo()
        {
            var result = new Dictionary<string, string>();
            foreach (var filename in Directory.GetFiles(ProjectDir, "*.csproj", SearchOption.AllDirectories))
            {
                if (filename.Contains("GodotXUnitApi"))
                    continue;
                result[Path.GetFileNameWithoutExtension(filename)] = filename;
            }
            return result;
        }

        public static string GetDefaultProject()
        {
            var project = Directory.GetFiles(ProjectDir, "*.csproj", SearchOption.TopDirectoryOnly);
            if (project.Length == 0)
            {
                GodotGD.PrintErr($"no csproj found on project root at {ProjectDir}. is this a mono project?");
                return "";
            }
            if (project.Length > 1)
            {
                GodotGD.PrintErr($"multiple csproj found on project root at {ProjectDir}.");
                return "";
            }
            return Path.GetFileNameWithoutExtension(project[0]);
        }
    }
}