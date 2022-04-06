using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public interface IReferToAVariable : ICannotBeCreated
    {
        VariableDescription VariableDescription { get; }
        
        void SetupPorts(VariableDescription description);
    }
}