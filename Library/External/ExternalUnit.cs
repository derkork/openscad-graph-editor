using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace OpenScadGraphEditor.Library.External
{
    /// <summary>
    /// This is a helper class that bundles a set of external references that are connected by include statements.
    /// </summary>
    public class ExternalUnit
    {
        private readonly ScadProject _project;

        /// <summary>
        /// The root reference which was included into the project.
        /// </summary>
        public ExternalReference Root { get; private set; }

        /// <summary>
        /// All references that are part of this external unit (including the root reference).
        /// </summary>
        public HashSet<ExternalReference> AllReferences { get; } = new HashSet<ExternalReference>();
        
        
        public ExternalUnit(ScadProject project)
        {
            _project = project;
        }

        private bool ContainsReferenceToSameFile(ExternalReference reference)
        {
            // resolve the full path to the reference
            if (!TryResolveReferenceFullPath(reference, out var fullPath))
            {
                // should not happen as we check this before we call this function, but if it does, we can't do anything
                throw new InvalidOperationException("Could not resolve full path for reference.");
            }
               
            return AllReferences.Any(r => TryResolveReferenceFullPath(r, out var existingFullPath) &&
                                          PathResolver.IsSamePath(fullPath, existingFullPath));
        }
        
        public void SetRootReference(ExternalReference reference)
        {
            if (Root != null)
            {
                throw new InvalidOperationException("Root reference already set.");
            }
            
            if (ContainsReferenceToSameFile(reference))
            {
                throw new InvalidOperationException("Root reference already added.");
            }
            
            Root = reference;
            AllReferences.Add(reference);
        }
        
        public void AddReferenceIfNotExists(ExternalReference reference)
        {
            if (!TryResolveReferenceFullPath(reference, out  _))
            {
                Log.Warning("Could not resolve full path for reference {Reference}. Ignoring it.", reference.IncludePath);
                // do nothing in this case
                return;
            }

            if (!ContainsReferenceToSameFile(reference))
            {
                AllReferences.Add(reference);
            }
        }
        
        private bool TryResolveReferenceFullPath(ExternalReference reference, out string fullPath)
        {
            var referenceOwner = reference.IsTransitive ? reference.IncludedBy : _project.ProjectPath;
            // resolve the full path of the reference
            var resolved = PathResolver.TryResolve(referenceOwner, reference.IncludePath, out fullPath);
            return resolved;
        }


    }
}