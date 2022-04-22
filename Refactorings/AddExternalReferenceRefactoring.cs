using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Refactorings
{
    public class AddExternalReferenceRefactoring  :Refactoring
    {
        private readonly string _includePath;
        private readonly IncludeMode _includeMode;

        public AddExternalReferenceRefactoring(string includePath, IncludeMode includeMode)
        {
            _includePath = includePath;
            _includeMode = includeMode;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            context.Project.AddReferenceToScadFile(_includePath, _includeMode);
        }
    }
}