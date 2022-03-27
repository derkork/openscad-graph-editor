using Godot;

namespace OpenScadGraphEditor.Refactorings
{
    public abstract class Refactoring : Reference
    {
        /// <summary>
        /// Performs the actual refactoring by modifying the the given project. All graphs in the project
        /// are guaranteed to be LightWeightGraphs when this is invoked.
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