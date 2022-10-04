using System.Collections.Generic;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public interface IReferToAVariable : ICannotBeCreated
    {
        VariableDescription VariableDescription { get; }
        
        void SetupPorts(VariableDescription description);
        
        /// <summary>
        /// Returns an enumerable of all ports in this which refer to the variable.
        /// </summary>
        IEnumerable<PortId> GetPortsReferringToVariable();
    }
}