using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Refactorings
{
    public class UpdateExternalReferenceRefactoring  :Refactoring
    {
        private readonly ExternalReference _reference;
        private readonly string _includePath;
        private readonly IncludeMode _includeMode;

        public UpdateExternalReferenceRefactoring(ExternalReference reference, string includePath, IncludeMode includeMode)
        {
            _reference = reference;
            _includePath = includePath;
            _includeMode = includeMode;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // set new values and then perform a refresh
            _reference.IncludePath = _includePath;
            _reference.Mode = _includeMode;
            
            context.PerformRefactoring(new AddOrUpdateExternalReferenceRefactoring(_reference));
        }
    }
}