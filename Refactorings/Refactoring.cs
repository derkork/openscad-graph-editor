namespace OpenScadGraphEditor.Refactorings
{
    public abstract class Refactoring
    {
        /// <summary>
        /// Performs the actual refactoring by modifying the the given project.
        /// </summary>
        public abstract void PerformRefactoring(RefactoringContext context);

        public virtual bool IsLate => false;
    }
}