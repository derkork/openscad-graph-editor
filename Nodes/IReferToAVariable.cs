using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public interface IReferToAVariable : ICannotBeCreated
    {
        void Setup(VariableDescription description);
    }
}