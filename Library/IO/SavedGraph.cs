using Godot;
using Godot.Collections;

namespace OpenScadGraphEditor.Library.IO
{
    /// <summary>
    /// This resource represents a module or function implementation as a node graph.
    /// </summary>
    public class SavedGraph : Resource
    {
        [Export]
        public SavedInvokableDescription Description;
        
        [Export]
        public Array<SavedNode> Nodes = new Array<SavedNode>();
        
        [Export]
        public Array<SavedConnection> Connections = new Array<SavedConnection>();
    }
}