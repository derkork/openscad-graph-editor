namespace OpenScadGraphEditor.Refactorings
{
    public abstract class Refactoring
    {
        /// <summary>
        /// Performs the actual refactoring by modifying the the given project.
        /// </summary>
        public abstract void PerformRefactoring(RefactoringContext context);

        /// <summary>
        /// Called after the refactoring is applied. Can be used to open certain items in the graph editor.
        /// </summary>
        /// <param name="graphEditor"></param>
        public virtual void AfterRefactoring(GraphEditor graphEditor)
        {
        }

        public virtual bool IsLate => false;
    }
}