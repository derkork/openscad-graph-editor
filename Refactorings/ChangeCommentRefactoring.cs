using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeCommentRefactoring : NodeRefactoring
    {
        private readonly string _title;
        private readonly string _description;

        public ChangeCommentRefactoring(ScadGraph holder, ScadNode node, string title, string description = "") : base(holder, node)
        {
            _title = title;
            _description = description;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (Node is Comment comment)
            {
                if (_title.Empty() && _description.Empty())
                {
                    context.PerformRefactoring(new DeleteNodeRefactoring(Holder, Node));
                }
                else // just update the comment
                {
                    comment.CommentTitle = _title;
                    comment.CommentDescription = _description;
                }
            }
            else
            {
                // normal nodes do not support description
                Node.SetComment(_title);
            }
        }
    }
}