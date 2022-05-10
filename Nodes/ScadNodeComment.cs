using Godot;

namespace OpenScadGraphEditor.Nodes
{
    public static class ScadNodeCommentExt
    {
        public static bool TryGetComment(this ScadNode node, out string comment)
        {
            return node.TryGetCustomAttribute("comment", out comment);
        }

        public static void SetComment(this ScadNode node, string comment)
        {
            if (comment.Empty())
            {
                node.UnsetCustomAttribute("comment");
            }
            else
            {
                node.SetCustomAttribute("comment", comment);
            }
        }
    }
}