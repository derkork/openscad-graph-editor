using System.Collections.Generic;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Interface to be implemented by nodes which refer to an invokable.
    /// </summary>
    public interface IReferToAnInvokable : ICannotBeCreated
    {
        InvokableDescription InvokableDescription { get; }
        
        void SetupPorts(InvokableDescription description);

        /// <summary>
        /// Returns the input port index that corresponds to the given parameter index. Returns -1 if no such port exists.
        /// </summary>
        int GetParameterInputPort(int parameterIndex);

        /// <summary>
        /// Returns the output port index that corresponds to the given parameter index. Returns -1 if no such port exists.
        /// </summary>
        int GetParameterOutputPort(int parameterIndex);

    }
}