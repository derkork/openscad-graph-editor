namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// Information about a usage point in the code. This contains the id of the graph and the id of the node
    /// that uses a thing.
    /// </summary>
    public class UsagePointInformation
    {
        public string Label { get; }
        public string GraphId { get; }
        public string NodeId { get; }

        public UsagePointInformation(string label, string graphId, string nodeId)
        {
            Label = label;
            GraphId = graphId;
            NodeId = nodeId;
        }
    }
}