namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Interface for nodes which have variable output size. These nodes will automatically get a refactoring
    /// which allows to add and remove outputs. Incompatible with <see cref="IHaveVariableOutputSize"/>.
    /// </summary>
    public interface IHaveVariableOutputSize
    {
        /// <summary>
        /// The current amount of outputs. Must always be > 0.
        /// </summary>
        int CurrentOutputSize { get; }

        /// <summary>
        /// The offset where the Output ports begin.
        /// </summary>
        int OutputPortOffset { get; }

        /// <summary>
        /// The title that should be used for the refactoring that adds a port.
        /// </summary>
        string AddRefactoringTitle { get; }

        /// <summary>
        /// The title that should be used for the refactoring that removes a port.
        /// </summary>
        string RemoveRefactoringTitle { get; }

        /// <summary>
        /// Called when a variable Output port should be added. The node should prepare the new Output port and any
        /// literals. The refactoring will automatically fix connections.
        /// </summary>
        void AddVariableOutputPort();

        /// <summary>
        /// Called when a variable Output port should be removed. The node should remove the Output port and any
        /// literals. The refactoring will automatically fix connections.
        /// </summary>
        void RemoveVariableOutputPort();
    }
}