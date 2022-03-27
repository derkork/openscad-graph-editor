namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// A refactoring that does nothing.
    /// </summary>
    public class NoOpRefactoring : Refactoring
    {
        public static readonly NoOpRefactoring Instance = new NoOpRefactoring();
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            // Do nothing
        }
    }
}