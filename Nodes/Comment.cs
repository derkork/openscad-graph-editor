using System;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    public class Comment : ScadNode, IHaveCustomWidget
    {
        public override string NodeTitle => "Comment";
        public override string NodeDescription => "Hey i am a comment!";

        public Vector2 Size { get; set; } = new Vector2(100, 100);

        public override void SaveInto(SavedNode node)
        {
            node.SetData("node_size.x", Size.x);
            node.SetData("node_size.y", Size.y);
            base.SaveInto(node);
        }

        public override void RestorePortDefinitions(SavedNode node, IReferenceResolver resolver)
        {
            Size = new Vector2((float) node.GetDataDouble("node_size.x", 100), (float) node.GetDataDouble("node_size.y", 100));
            base.RestorePortDefinitions(node, resolver);
        }

        public override string Render(IScadGraph context)
        {
            throw new InvalidOperationException("Comment node cannot be rendered.");
        }

        public ScadNodeWidget InstantiateCustomWidget()
        {
            return Prefabs.New<CommentWidget>();
        }
    }
}