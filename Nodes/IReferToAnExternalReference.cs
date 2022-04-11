using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Interface to be implemented by nodes which refer to an invokable.
    /// </summary>
    public interface IReferToAnExternalReference : ICannotBeCreated
    {
        ExternalReference ExternalReference { get; }
        
        void SetupPorts(ExternalReference externalReference);
    }
}