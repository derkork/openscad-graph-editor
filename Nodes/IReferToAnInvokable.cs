using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    public interface IReferToAnInvokable : ICannotBeCreated
    {
        void Setup(InvokableDescription description);
    }
}