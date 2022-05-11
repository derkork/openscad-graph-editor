using System;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Widgets.CommentEditingDialog
{
    [UsedImplicitly]
    public class CommentEditingDialog : WindowDialog
    {
        public event Action<RequestContext, string, string> CommentAndTitleChanged;

        private LineEdit _titleEdit;
        private Label _descriptionLabel;
        private TextEdit _descriptionEdit;
        private RequestContext _requestContext;
        private bool _showDescription;

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

        public async void Open(RequestContext requestContext, string title = "", string description = "", bool showDescription = true)
        {
            _requestContext = requestContext;
            _titleEdit.Text = title;
            _descriptionEdit.Text = description;
            _showDescription = showDescription;
        
            _descriptionLabel.Visible = showDescription;
            _descriptionEdit.Visible = showDescription;
            
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

            CommentAndTitleChanged?.Invoke(_requestContext, title, _showDescription ? description : "");

            Hide();
        }
        
    }
}