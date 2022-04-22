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
                GD.Print($"Reference to '{fullPath}' already exists in project.");
                return;
            }

            string text;
            try
            {
                // read text from file
                text = System.IO.File.ReadAllText(fullPath, Encoding.UTF8);
            }
            catch (Exception e)
            {
                GD.PrintErr("Cannot read file. " + e.Message);
                // TODO better error handling
                return;
            }

            // build an external reference to parse into
            var externalReference = ExternalReferenceBuilder.Build(mode, includePath, owner);
            try
            {
                GD.Print("Parsing SCAD file: " + fullPath);
                // parse contents and fill the reference with data
                ExternalFileParser.Parse(text, externalReference);
            }
            catch (Exception e)
            {
                GD.PrintErr("Cannot parse file. " + e.Message);
                // TODO better error handling
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
            var referencesToRemoveDict = 
                referencesToRemove.ToDictionary(it => project.TryGetFullPathTo(it, out var fullPath) ? fullPath : null, it => it);
            
            // now get the set of references that are actually used by the project
            var referencesInUse = project.ExternalReferences.Except(referencesToRemove).ToHashSet();
            
            // get a dictionary of their full paths as well
            var referencesInUseDict = 
                referencesInUse.ToDictionary(it => project.TryGetFullPathTo(it, out var fullPath) ? fullPath : null, it => it);


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