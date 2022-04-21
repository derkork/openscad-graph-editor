using System;
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

            if (context.Project.ContainsReferenceTo(fullPath))
            {
                // nothing to do, we already got this.
            }

            // if it doesn't exist, load the file and add it to the project.
            ParseAndAdd(context, _externalReference, fullPath);
        }

        private void ParseAndAdd(RefactoringContext context, ExternalReference reference, string fullPath)
        {
            try
            {
                // read text from file
                var text = System.IO.File.ReadAllText(fullPath, Encoding.UTF8);
                GD.Print("Parsing SCAD file: " + fullPath);
                // parse contents and fill the reference with data
                ExternalFileParser.Parse(text, reference);

                // and add it to the project
                context.Project.AddExternalReference(reference);
                
                
                // check if the reference again contains includes
                var directory = PathResolver.DirectoryFromFile(fullPath);

                foreach (var includedReference in reference.References)
                {
                    var resolved = PathResolver.TryResolve(directory, includedReference, out var includedFullPath);
                    if (!resolved)
                    {
                        // TODO: better error handling
                        GD.PrintErr("Cannot resolve transitive reference to '{0}'", includedReference);
                        continue;
                    }
                    
                    // check if we already have this reference
                    if (context.Project.ContainsReferenceTo(includedFullPath))
                    {
                        // we got this
                        continue;
                    }
                    
                    // if not, add another one
                    var transitiveReference = ExternalReferenceBuilder.Build(IncludeMode.Include, includedReference, reference );
                    ParseAndAdd(context, transitiveReference, includedFullPath);
                }
            }
            catch (Exception e)
            {
                GD.PrintErr("Cannot read file. " + e.Message);
                // TODO: better error handling
            }
        }
    }
}