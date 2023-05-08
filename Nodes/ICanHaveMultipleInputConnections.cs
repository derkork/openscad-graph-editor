namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Marker interface for nodes that accept multiple input connections. For these nodes, the rule that
    /// a port can only have one input connection does not apply. The nodes are themselves responsible to
    /// create appropriate rules governing their input connections.
    /// </summary>
    public interface ICanHaveMultipleInputConnections
    {
    }
}