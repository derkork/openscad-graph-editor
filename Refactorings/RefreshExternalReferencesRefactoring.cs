using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    public class RefreshExternalReferencesRefactoring : Refactoring
    {
        public override void PerformRefactoring(RefactoringContext context)
        {
            // we will reimport all external references
            var filesToImport = new Dictionary<string, IncludeMode>();
            // we only look at the top level imports, all transitive imports will be just deleted and then
            // re-added should they still exist in the files
            context.Project.ExternalReferences
                .Where(it => !it.IsTransitive)
                .ForAll(import => filesToImport[import.IncludePath] = import.Mode);
            
            // we have now a new list of files to import, so we can just remove all imports and then add them again
            context.PerformRefactoring(new ReplaceExternalReferencesRefactoring(filesToImport));
        }
    }
}