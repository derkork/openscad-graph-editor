using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Widgets;
using Serilog;
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
        private static IEnumerable<string> GetLibraryPaths()
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
        /// Returns the name of the OpenScad executable for the current operating system.
        /// </summary>
        public static string GetOpenScadExecutableName()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                    return "openscad.exe";
                case PlatformID.Unix:
                    return "openscad";
                case PlatformID.MacOSX:
                    return "OpenScad.app";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        /// <summary>
        /// Checks whether two paths refer to the same file. Both paths must be absolute.
        /// </summary>
        public static bool IsSamePath(string path1, string path2)
        {
            GdAssert.That(Path.IsPathRooted(path1), "Path1 is not absolute");
            GdAssert.That(Path.IsPathRooted(path2), "Path2 is not absolute");

            // Uri does path canonicalization. So we can use that to check if the paths are the same.
            // https://stackoverflow.com/questions/1266674/how-can-one-get-an-absolute-or-normalized-file-path-in-net
            return new Uri(path1).LocalPath == new Uri(path2).LocalPath;
        }


        /// <summary>
        /// Tries to resolve a path from an include statement. Returns a canonical, normalized path.
        /// </summary>
        public static bool TryResolve([CanBeNull] string pathToSourceFile, string includePath,
            out string resolvedFullPath)
        {
            var sourceDirectory = pathToSourceFile.Empty() ? "" : Path.GetDirectoryName(pathToSourceFile);

            if (!sourceDirectory.Empty())
            {
                GdAssert.That(Path.IsPathRooted(sourceDirectory), "Source path is not absolute.");
            }

            var isAbsolute = Path.IsPathRooted(includePath);


            if (isAbsolute)
            {
                resolvedFullPath = new Uri(includePath).LocalPath;
                return File.Exists(resolvedFullPath);
            }

            // first try to find the file relative to the include location
            if (!sourceDirectory.Empty())
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var tryResolve = Path.Combine(sourceDirectory, includePath);
                if (File.Exists(tryResolve))
                {
                    resolvedFullPath = new Uri(tryResolve).LocalPath;
                    return true;
                }
            }

            // no joy, then try to find it in the library paths
            foreach (var libraryPath in GetLibraryPaths())
            {
                var tryResolve = Path.Combine(libraryPath, includePath);
                if (!File.Exists(tryResolve))
                {
                    continue;
                }

                resolvedFullPath = new Uri(tryResolve).LocalPath;
                return true;
            }

            // still no joy, give up.

            resolvedFullPath = default;
            return false;
        }


        /// <summary>
        /// Returns the user's documents folder.
        /// </summary>
        public static string GetUsersDocumentsFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        /// <summary>
        /// Calculates the given <see cref="absolutePath"/> relative to the given <see cref="relativeToDirectory"/> path.
        /// </summary>
        public static bool TryAbsoluteToRelative(string absolutePath, string relativeToDirectory, out string result)
        {
            var absoluteDirectoryParts = NormalizePath(absolutePath).Split('/');
            var relativeToDirectoryParts = NormalizePath(relativeToDirectory).Split('/');

            // Get the shortest of the two paths
            var len = Mathf.Min(absoluteDirectoryParts.Length, relativeToDirectoryParts.Length);

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
            // exclude the last element as this is a file name.
            for (var index = lastCommonRoot + 1; index < relativeToDirectoryParts.Length; index++)
            {
                if (absoluteDirectoryParts[index].Length > 0)
                {
                    relativePath.Append("../");
                }
            }

            // Add on the folders
            for (var index = lastCommonRoot + 1; index < absoluteDirectoryParts.Length - 1; index++)
            {
                relativePath.Append(absoluteDirectoryParts[index] + "/");
            }

            relativePath.Append(absoluteDirectoryParts[absoluteDirectoryParts.Length - 1]);

            result = relativePath.ToString();
            return true;
        }

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }


        /// <summary>
        /// Returns all library files relative to the library root folder.
        /// </summary>
        public static string[] GetAllLibraryFiles()
        {
            var paths = GetLibraryPaths();
            return paths.SelectMany(path =>
                {
                    try
                    {
                        if (Directory.Exists(path))
                        {
                            return Directory.GetFiles(path, "*.scad", SearchOption.AllDirectories)
                                .Select(it =>
                                {
                                    if (TryAbsoluteToRelative(it, path, out var result))
                                    {
                                        return result;
                                    }

                                    ;
                                    NotificationService.ShowError("Couldn't convert " + it + " to relative path.");
                                    return "";
                                })
                                .Where(it => !it.Empty());
                        }
                    }
                    catch (Exception)
                    {
                        Log.Warning("Could not read library files from {Path}", path);
                    }

                    return Array.Empty<string>();
                })
                .ToArray();
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