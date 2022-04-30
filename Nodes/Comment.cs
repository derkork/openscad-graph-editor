using System;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Nodes
{
    public class Comment : ScadNode, IHaveCustomWidget
    {
        public override string NodeTitle => "Comment";
        public override string NodeDescription => "Hey i am a comment!";
        
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