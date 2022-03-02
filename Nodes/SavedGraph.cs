using Godot;
using Godot.Collections;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// This resource represents a saved graph
    /// </summary>
    public class SavedGraph : Resource
    {
        [Export]
        public Array<SavedNode> Nodes = new Array<SavedNode>();
        
        [Export]
        public Array<SavedConnection> Connections = new Array<SavedConnection>();
    }
}