namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Interface for nodes which have variable input size. These nodes will automatically get a refactoring
    /// which allows to add and remove inputs.
    /// </summary>
    public interface IHaveVariableInputSize
    {
        /// <summary>
        /// The current amount of inputs. Must always be > 0.
        /// </summary>
        int CurrentInputSize { get; }

        /// <summary>
        /// The offset where the input ports begin.
        /// </summary>
        int InputPortOffset { get; }

        /// <summary>
        /// The offset where the output ports begin. Ignored when <see cref="OutputPortsMatchVariableInputs"/> returns
        /// false.s
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
        /// Called when a variable input port should be added. The node should prepare the new input port and any
        /// literals. The refactoring will automatically fix connections.
        /// </summary>
        void AddVariableInputPort();

        /// <summary>
        /// Called when a variable input port should be removed. The node should remove the input port and any
        /// literals. The refactoring will automatically fix connections.
        /// </summary>
        void RemoveVariableInputPort();

        /// <summary>
        /// If true, the node has one output port for each variable input port, otherwise, the node has no variable
        /// output ports.
        /// </summary>
        bool OutputPortsMatchVariableInputs { get; }
    }
}