using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    public class ChangeCommentRefactoring : NodeRefactoring
    {
        private readonly string _title;
        private readonly string _description;

        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        public ChangeCommentRefactoring(IScadGraph holder, Comment node, string title, string description) : base(holder, node)
        {
            _title = title;
            _description = description;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var reference = context.MakeRefactorable(Holder, Node);
            var comment = (Comment)reference.Node;

            comment.CommentTitle = _title;
            comment.CommentDescription = _description;

        }
    }
}