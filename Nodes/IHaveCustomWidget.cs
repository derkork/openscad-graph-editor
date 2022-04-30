using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Marker interface for nodes that use a custom widget to render themselves.
    /// </summary>
    public interface IHaveCustomWidget
    {
        ScadNodeWidget InstantiateCustomWidget();
    }
}