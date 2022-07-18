using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;
using Serilog;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Dependency management extensions for the ScadProject.
    /// </summary>
    public static class DependencyExt
    {
        /// <summary>
        /// Adds a reference to a scad file to the project using the given include mode.
        /// </summary>
        public static void AddReferenceToScadFile(this ScadProject project, string includePath, IncludeMode mode)
        {
            project.AddReferenceToScadFile(project.ProjectPath,  includePath, mode);
        }

        private static void AddReferenceToScadFile(this ScadProject project, string sourceFile, string includePath, IncludeMode mode,
            [CanBeNull] ExternalReference owner = null)
        {
            GdAssert.That(owner == null || mode == IncludeMode.Include, "We should not have include mode 'use' for a transitive reference.");
            var resolved = PathResolver.TryResolve(sourceFile, includePath, out var fullPath);
            GdAssert.That(resolved, $"Could not resolve reference '{sourceFile}'");
            
            // now check if we have any external reference with the same path in the project already
            if (project.ExternalReferences.Any(it => project.TryGetFullPathTo(it, out var fullPath2) && PathResolver.IsSamePath(fullPath2, fullPath)))
            {
                // if so, we can skip this one.
                Log.Information("Reference to {FullPath} already exists in project", fullPath);
                return;
            }
            // build an external reference to parse into
            var externalReference = ExternalReferenceBuilder.Build(mode, includePath, owner);

            if (!externalReference.ParseFile(fullPath))
            {
                return;
            }

            // and add it to the project
            project.AddExternalReference(externalReference);
                
                
            // now recursively add all transitive references
            foreach (var transitiveReference in externalReference.References)
            {
                AddReferenceToScadFile(project, fullPath, transitiveReference, IncludeMode.Include, externalReference);
            }
        }

        public static bool TryResolveFullPath(this ExternalReference reference, ScadProject project, out string result)
        {
            // first we need to find out if the reference is included from the root or was included by another reference
            if (reference.IncludedBy.Empty())
            {
                // this is a root reference.
                return PathResolver.TryResolve(project.ProjectPath, reference.IncludePath, out result);
            }
            
            // this is a transitive reference.
            // we first determine the full path of the parent reference and then use that to resolve the path of this reference
            var previousReference = project.ResolveExternalReference(reference.IncludedBy);
            if (TryResolveFullPath(previousReference, project, out var fullPathToParent))
            {
                // now we got everything
                return PathResolver.TryResolve(fullPathToParent, reference.IncludePath, out result);
            }
            
            // we could not resolve the full path
            result = default;
            return false;
        }
        
        public static bool ParseFile(this ExternalReference externalReference, string fullPath)
        {
            GdAssert.That(!externalReference.IsLoaded, "External reference is already loaded.");
            
            string text;
            try
            {
                // read text from file
                text = System.IO.File.ReadAllText(fullPath, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Log.Warning(e,"Cannot read file {Path}", fullPath);
                return false;
            }

            try
            {
                Log.Information("Parsing SCAD file {Path} ", fullPath);
                // parse contents and fill the reference with data
                ExternalFileParser.Parse(text, externalReference);
            }
            catch (Exception e)
            {
                Log.Warning(e,"Cannot parse file {Path}", fullPath);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Prepares a reference for removal. Returns a list of all references that will be removed.
        /// The list may contain more than one item if the reference included other references. The function will ensure
        /// that transitive references that are included by more than one file will remain in project as long as they
        /// are still referenced by other files. The the caller is responsible for deleting all data that still belongs
        /// to these references and to ultimately remove the references from the project.
        /// </summary>
        public static IEnumerable<ExternalReference> PrepareForRemoval(this ScadProject project, ExternalReference externalReference)
        {
            // first gather a set of all external references that would be removed by this (e.g. the reference itself and all transitive references)
            var referencesToRemove = project.GetTransitiveReferences(externalReference);
            
            // now make a dictionary which has the full path to the
            // external reference as key and the reference itself as value
            // use the relative path in case we cannot get the full path (because the file doesn't exist anymore)
            var referencesToRemoveDict = 
                referencesToRemove.ToDictionary(it => project.TryGetFullPathTo(it, out var fullPath) ? fullPath : it.IncludePath, it => it);
            
            // now get the set of references that are actually used by the project
            var referencesInUse = project.ExternalReferences.Except(referencesToRemove).ToHashSet();
            
            // get a dictionary of their full paths as well
            // again, use the relative path in case we cannot get the full path (because the file doesn't exist anymore)
            var referencesInUseDict = 
                referencesInUse.ToDictionary(it => project.TryGetFullPathTo(it, out var fullPath) ? fullPath : it.IncludePath, it => it);


            // now walk over the reference we keep and try to find out if it refers to one of the references
            // that we want to delete.
            foreach (var item in referencesInUseDict)
            {
                var path = item.Key;
                var reference = item.Value;

                // get all references that this reference refers to
                foreach (var transitivePath in reference.References)
                {
                    // make a full path to the transitive reference
                    if (PathResolver.TryResolve(path, transitivePath, out var resolvedPath))
                    {
                        // if the path is in the list of references to remove, then keep the reference and change its owner
                        if (referencesToRemoveDict.TryGetValue(resolvedPath, out var referenceStillInUse))
                        {
                            referencesToRemoveDict.Remove(resolvedPath);
                            referenceStillInUse.IncludedBy = reference.Id;
                        }
                    }
                }
                
                if (referencesToRemoveDict.Count == 0)
                {
                    // no need to continue if we already removed all references
                    break;
                }
            }
            
            return referencesToRemoveDict.Values;
        }
        
        /// <summary>
        /// Returns all transitive references of the given reference. The given reference is included in the result.
        /// </summary>
        private static HashSet<ExternalReference> GetTransitiveReferences(this ScadProject project, ExternalReference externalReference)
        {
            var references = new HashSet<ExternalReference>();
            var queue = new Queue<ExternalReference>();
            queue.Enqueue(externalReference);
            while (queue.Count > 0)
            {
                var reference = queue.Dequeue();
                references.Add(reference);
                // now find all references that were included by this reference
                // and enqueue them
                project.ExternalReferences
                    .Where(it => it.IncludedBy == reference.Id)
                    .ForAll(it => queue.Enqueue(it));
            }
            return references;
        }
        
        
        
        
        private static bool TryGetFullPathTo(this ScadProject project, ExternalReference reference, out string result)
        {
            if (reference.IsTransitive)
            {
                var includedId = reference.IncludedBy;
                var includedBy = project.ResolveExternalReference(includedId);
                // get its full path, then resolve 
                if (TryGetFullPathTo(project, includedBy, out var includedByPath))
                {
                    if (PathResolver.TryResolve(includedByPath, reference.IncludePath, out result))
                    {
                        return true;
                    }
                }
            }
            
            // if it is not transitive, try to resolve it relative to this project
            if (PathResolver.TryResolve(project.ProjectPath, reference.IncludePath, out result))
            {
                return true;
            }

            result = default;
            return false;
        }

    }
}