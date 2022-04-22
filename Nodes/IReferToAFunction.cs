namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Interface to be implemented by nodes which refer to functions.
    /// </summary>
    public interface IReferToAFunction : IReferToAnInvokable
    {

        /// <summary>
        /// Returns the input port which represents the return value of the function. Returns -1 if no such
        /// port exists. Like with the other port mapping functions this function is supposed to not change
        /// its output when the given Invokable is changed.
        /// </summary>
        int GetReturnValueInputPort();

        /// <summary>
        /// Returns the output port which represents the return value of the function. Returns -1 if no such
        /// port exists. Like with the other port mapping functions this function is supposed to not change
        /// when the given Invokable is changed.
        /// </summary>
        int GetReturnValueOutputPort();
    }
}