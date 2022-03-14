using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Marker interface for nodes that have multiple expression output (e.g. function entry points, for-loops).
    /// They have the capability to render different results based on which expression port is queried.
    /// </summary>
    public interface IMultiExpressionOutputNode
    {
        /// <summary>
        /// Instructs the node to only render the expression output with the given index.
        /// </summary>
        string RenderExpressionOutput(IScadGraph context, int port);
    }
}