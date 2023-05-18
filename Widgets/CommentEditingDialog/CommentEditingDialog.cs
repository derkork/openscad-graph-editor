using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.CommentEditingDialog
{
    [UsedImplicitly]
    public class CommentEditingDialog : WindowDialog
    {
        
        private LineEdit _titleEdit;
        private Label _descriptionLabel;
        private TextEdit _descriptionEdit;
        private ScadNode _node;
        private ScadGraph _graph;
        private Vector2 _position;
        private IEditorContext _context;

        public override void _Ready()
        {
            _titleEdit = this.WithName<LineEdit>("TitleEdit");
            _descriptionEdit = this.WithName<TextEdit>("DescriptionEdit");
            _descriptionLabel = this.WithName<Label>("DescriptionLabel");
            
            _titleEdit
                .Connect("text_entered")
                .To(this, nameof(OnTextEntered));
            
            // connect remove comment button
            this.WithName<Button>("RemoveCommentButton")
                .Connect("pressed")
                .To(this, nameof(OnRemoveCommentButtonPressed));
            
            // connect cancel button
            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelButtonPressed));
            
            // connect OK button
            this.WithName<Button>("OKButton")
                .Connect("pressed")
                .To(this, nameof(OnOkButtonPressed));
            
        }

        public void Open(IEditorContext context, ScadGraph graph, ScadNode node)
        {
            _node = node;
            _graph = graph;
            _context = context;
            
            if (node is Comment comment)
            {
                _titleEdit.Text = comment.CommentTitle;
                _descriptionEdit.Text = comment.CommentDescription;
                _descriptionLabel.Visible = true;
                _descriptionEdit.Visible = true;
            }
            else
            {
                _titleEdit.Text = "";
                if (node.TryGetComment(out var commentText))
                {
                    _titleEdit.Text = commentText;
                }
                _descriptionEdit.Text = "";
                _descriptionLabel.Visible = false;
                _descriptionEdit.Visible = false;
            }
            
            SetAsMinsize();
            PopupCenteredMinsize();
            _titleEdit.GrabFocus();
        }

        
        public void OpenForCreation(ScadGraph graph, Vector2 position)
        {
            _node = null;
            _graph = graph;
            _position = position;
            
            _titleEdit.Text = "";
            _descriptionEdit.Text = "";
            _descriptionLabel.Visible = true;
            _descriptionEdit.Visible = true;
            
            SetAsMinsize();
            PopupCenteredMinsize();
            _titleEdit.GrabFocus();
        }

        private void OnCancelButtonPressed()
        {
            Hide();
        }

        private void OnRemoveCommentButtonPressed()
        {
            _descriptionEdit.Text = "";
            _titleEdit.Text = "";
            OnOkButtonPressed();
        }

        public void OnTextEntered([UsedImplicitly] string _)
        {
            // when the user presses enter in the title edit, we assume he wants to close the dialog
            OnOkButtonPressed();
        } 

        private void OnOkButtonPressed()
        {
            var title = _titleEdit.Text;
            var description = _descriptionEdit.Text;
            
            // if we have no node, we first need to create one, but only if at least one of the fields is not empty
            if (_node == null && (title.Empty() == false || description.Empty() == false))
            {
                var comment = NodeFactory.Build<Comment>();
                comment.CommentTitle = title;
                comment.CommentDescription = description;
                comment.Offset = _position;
                _context.PerformRefactoring("Create comment",  new AddNodeRefactoring(_graph, comment));
            }
            else if (_node != null)
            {
                // if both fields are empty, we delete the comment
                // otherwise we just update the comment
                var actionText = title.Empty() && description.Empty() ? "Delete comment" : "Update comment";
                _context.PerformRefactoring(actionText, new ChangeCommentRefactoring(_graph, _node, title, description));
            }

            Hide();
        }
        
    }
}