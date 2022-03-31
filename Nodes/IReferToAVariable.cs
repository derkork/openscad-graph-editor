using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public interface IReferToAVariable : ICannotBeCreated
    {
        void SetupPorts(VariableDescription description);
    }
}