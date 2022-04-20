using System;
using System.Linq;
using System.Text;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Refactorings
{
    public class AddExternalReferenceRefactoring  :Refactoring
    {
        private readonly ExternalReference _externalReference;

        public AddExternalReferenceRefactoring(ExternalReference externalReference)
        {
            _externalReference = externalReference;
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var projectFile = context.Project.ProjectPath;
            var projectDirectory = projectFile != null ? PathResolver.DirectoryFromFile(projectFile) : null;

            // first, check if the reference is already in the project.
            var resolved = PathResolver.TryResolve(projectDirectory, _externalReference.SourceFile, out var fullPath);
            GdAssert.That(resolved, $"Could not resolve reference '{_externalReference.SourceFile}'");

            var exists = context.Project.ExternalReferences.FirstOrDefault(it =>
            {
                var innerResolved = PathResolver.TryResolve(projectDirectory, it.SourceFile, out var innerFullPath);
                GdAssert.That(innerResolved, $"Could not resolve reference '{it.SourceFile}'");

                return PathResolver.IsSamePath(fullPath, innerFullPath);
            });

            if (exists == null)
            {
                // if it doesn't exist, load the file and add it to the project.
                try
                {
                    // read text from file
                    var text = System.IO.File.ReadAllText(fullPath, Encoding.UTF8);
                    // parse contents and fill the reference with data
                    ExternalFileParser.Parse(text, _externalReference);

                    // and add it to the project
                    context.Project.AddExternalReference(_externalReference);
                }
                catch (Exception e)
                {
                    GD.PrintErr("Cannot read file. " + e.Message);
                    // TODO: better error handling
                }
            }
        }
    }
}