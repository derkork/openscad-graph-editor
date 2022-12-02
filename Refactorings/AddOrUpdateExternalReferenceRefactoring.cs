using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring which refreshes code from an external reference.
    /// </summary>
    public class AddOrUpdateExternalReferenceRefactoring : Refactoring
    {
        private readonly string _includePath;
        private readonly IncludeMode _includeMode;

        public AddOrUpdateExternalReferenceRefactoring(string includePath, IncludeMode includeMode)
        {
            _includePath = includePath;
            _includeMode = includeMode;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // plan of attack: since changing the imports can have all kinds of interesting edge cases, we'll just
            // completely remove all imports, regenerate a list of new imports and then import them all again.
            // we could be smarter about this, but there are so many edge cases (e.g. functions moving between files,
            // functions being renamed, user changing an import to a different file, etc.) that the complexity of trying
            // to be smart is not worth it.
            
            // first make a map of all the files we have to import, key is include path, value is include mode
            var filesToImport = new Dictionary<string, IncludeMode>();
            // we only look at the top level imports, all transitive imports will be just deleted and then
            // re-added should they still exist in the files
            context.Project.ExternalReferences
                .Where(it => !it.IsTransitive)
                .ForAll(import => filesToImport[import.IncludePath] = import.Mode);
            
            // now we will check if the new file we want to import is already in the list
            // for this we need to check if the include path resolves to the same file
            if (!PathResolver.TryResolve(context.Project.ProjectPath, _includePath, out var fullPathToNewInclude))
            {
                NotificationService.ShowError("Cannot find file to include at " + _includePath);
                return;
            }
            
            // now we need to check if the file is already in the list
            var existingInclude = filesToImport.Where(it => 
                PathResolver.TryResolve(context.Project.ProjectPath, it.Key, out var fullPathToExistingInclude)
                && PathResolver.IsSamePath(fullPathToNewInclude, fullPathToExistingInclude))
                .Select(it => it.Key)
                .FirstOrDefault();
            
            // if  it is, we need to update the include mode, otherwise we just add it
            if (existingInclude != null)
            {
                filesToImport[existingInclude] = _includeMode;
            }
            else
            {
                filesToImport[_includePath] = _includeMode;
            }
            
            // we have now a new list of files to import, so we can just remove all imports and then add them again
            context.PerformRefactoring(new ReplaceExternalReferencesRefactoring(filesToImport));
        }
    }
}