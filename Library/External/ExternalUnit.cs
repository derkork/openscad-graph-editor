using System;
using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Refactorings;
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
        /// All references that are part of this external unit (including the root reference).
        /// </summary>
        public HashSet<ExternalReference> AllReferences { get; } = new HashSet<ExternalReference>();
        
        private readonly HashSet<string> _allIncludePaths = new HashSet<string>();


        public ExternalUnit(ScadProject project)
        {
            _project = project;
        }

        public void AddReferenceIfNotExists(ExternalReference reference, string fullPath)
        {
            if (HasPath(fullPath))
            {
                return;
            }

            AllReferences.Add(reference);
            _allIncludePaths.Add(fullPath);
        }

        public bool HasPath(string fullPath)
        {
            return _allIncludePaths.Any(existingFullPath => PathResolver.IsSamePath(fullPath, existingFullPath));
        }
    }
}