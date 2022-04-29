using System.Collections.Generic;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Interface to be implemented by nodes which refer to functions.
    /// </summary>
    public interface IReferToAFunction : IReferToAnInvokable
    {
        /// <summary>
        /// Returns an enumerable of all ports in this node which refer to the return value of the function.
        /// </summary>
        IEnumerable<PortId> GetPortsReferringToReturnValue();
    }
}