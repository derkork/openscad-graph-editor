using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Godot;
using Directory = System.IO.Directory;
using Environment = System.Environment;
using File = System.IO.File;
using Path = System.IO.Path;

namespace OpenScadGraphEditor.Library.External
{
    /// <summary>
    /// Resolver for paths to OpenScad libraries (see https://en.wikibooks.org/wiki/OpenSCAD_User_Manual/Libraries).
    /// </summary>
    public static class PathResolver
    {
        public static IEnumerable<string> GetLibraryPaths()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return GetWindowsLibraryPaths();
                case PlatformID.Unix:
                    return GetLinuxLibraryPaths();
                case PlatformID.MacOSX:
                    return GetMacLibraryPaths();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Encodes the given path to a representation that allows resolving it.
        /// </summary>
        public static string Encode(string path, ExternalFilePathMode mode)
        {
            var normalizedPath = NormalizePath(path);
            switch (mode)
            {
                case ExternalFilePathMode.Library:
                    return $"library://{normalizedPath}";
                case ExternalFilePathMode.Relative:
                    return $"relative://{normalizedPath}";
                case ExternalFilePathMode.Absolute:
                    return $"absolute://{normalizedPath}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        /// <summary>
        /// Tries to resolve a path that was encoded with <see cref="Encode"/>.
        /// </summary>
        public static bool TryResolve(string projectPath, string path, out string resolvedPath)
        {
            if (path.StartsWith("library://"))
            {
                var file = path.Substring("library://".Length);
                var libraryPaths = GetLibraryPaths();
                var realPath = libraryPaths
                    .Select(it => Path.Combine(it, file))
                    .Where(File.Exists)
                    .FirstOrDefault();

                if (realPath != null)
                {
                    resolvedPath = realPath;
                    return true;
                }
            }

            if (path.StartsWith("relative://"))
            {
                var file = path.Substring("relative://".Length);
                if (File.Exists(Path.Combine(projectPath, file)))
                {
                    resolvedPath = Path.Combine(projectPath, file);
                    return true;
                }
            }


            if (path.StartsWith("absolute://"))
            {
                var file = path.Substring("absolute://".Length);
                if (File.Exists(file))
                {
                    resolvedPath = file;
                    return true;
                }
            }

            resolvedPath = default;
            return false;
        }

        /// <summary>
        /// Return s
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string DirectoryFromFile(string filePath)
        {
            return Path.GetDirectoryName(filePath);
        }


        /// <summary>
        /// Returns the user's documents folder.
        /// </summary>
        public static string GetUsersDocumentsFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        
        /// <summary>
        /// Calculates the given <see cref="absolutePath"/> relative to the given <see cref="relativeTo"/> path.
        /// </summary>
        public static bool TryAbsoluteToRelative(string absolutePath, string relativeTo, out string result)
        {
            var absoluteDirectoryParts = NormalizePath(absolutePath).Split('/');
            var relativeToDirectoryParts = NormalizePath(relativeTo).Split('/');

            // Get the shortest of the two paths
            var len = absoluteDirectoryParts.Length < relativeToDirectoryParts.Length ? absoluteDirectoryParts.Length : relativeToDirectoryParts.Length;

            // Use to determine where in the loop we exited
            var lastCommonRoot = -1;

            // Find common root
            for (var index = 0; index < len; index++)
            {
                if (absoluteDirectoryParts[index] == relativeToDirectoryParts[index])
                {
                    lastCommonRoot = index;
                }
                else break;
            }

            // If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
            {
                result = default;
                return false;
            }

            // Build up the relative path
            var relativePath = new StringBuilder();

            // Add on the ..
            for (var index = lastCommonRoot + 1; index < absoluteDirectoryParts.Length; index++)
            {
                if (absoluteDirectoryParts[index].Length > 0)
                {
                    relativePath.Append("../");
                }
            }

            // Add on the folders
            for (var index = lastCommonRoot + 1; index < relativeToDirectoryParts.Length - 1; index++)
            {
                relativePath.Append(relativeToDirectoryParts[index] + "/");
            }

            relativePath.Append(relativeToDirectoryParts[relativeToDirectoryParts.Length - 1]);

            result = relativePath.ToString();
            return true;
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }


        public static string[] GetAllLibraryFiles()
        {
            var paths = GetLibraryPaths();
            return paths.SelectMany(path =>
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        return Directory.GetFiles(path, "*.scad", SearchOption.AllDirectories);
                    }
                }
                catch (Exception)
                {
                    GD.Print("Could not read library files from " + path);
                }

                return Array.Empty<string>();
            }).ToArray();
        }

        private static IEnumerable<string> GetWindowsLibraryPaths()
        {
            var osSpecificLibraryPath = NormalizePath(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            return Enumerable.Empty<string>().Append(osSpecificLibraryPath).Concat(GetUserLibraryPaths());
        }

        private static IEnumerable<string> GetLinuxLibraryPaths()
        {
            var osSpecificLibraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local/share/OpenSCAD/libraries");
            return Enumerable.Empty<string>().Append(osSpecificLibraryPath).Concat(GetUserLibraryPaths());
        }

        private static IEnumerable<string> GetMacLibraryPaths()
        {
            var osSpecificLibraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Documents/OpenSCAD/libraries");
            return Enumerable.Empty<string>().Append(osSpecificLibraryPath).Concat(GetUserLibraryPaths());
        }


        private static IEnumerable<string> GetUserLibraryPaths()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("OPENSCADPATH");
            return string.IsNullOrEmpty(environmentVariable)
                ? Enumerable.Empty<string>()
                : environmentVariable.Split(Path.PathSeparator).Select(NormalizePath);
        }
    }
}