namespace OpenScadGraphEditor.Nodes
{
    public interface IAmBoundToOtherNode : ICannotBeCreated
    {
        string OtherNodeId { get; set; }
    }
}