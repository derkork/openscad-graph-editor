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

        private Label _titleLabel;
        private LineEdit _titleEdit;
        private TextEdit _descriptionEdit;
        private RequestContext _requestContext;
        private bool _showTitle;

        public override void _Ready()
        {
            _titleEdit = this.WithName<LineEdit>("TitleEdit");
            _titleLabel = this.WithName<Label>("TitleLabel");
            _descriptionEdit = this.WithName<TextEdit>("DescriptionEdit");
            
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

        public void Open(RequestContext requestContext, string title = "", string description = "", bool showTitle = true)
        {
            _requestContext = requestContext;
            _titleEdit.Text = title;
            _descriptionEdit.Text = description;
            _showTitle = showTitle;
            
            _titleEdit.Visible = showTitle;
            _titleLabel.Visible = showTitle;
            
            PopupCenteredMinsize();
            if (_showTitle)
            {
                _titleEdit.GrabFocus();
            }
            else
            {
                _descriptionEdit.GrabFocus();
            }
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

        private void OnOkButtonPressed()
        {
            var title = _titleEdit.Text;
            var description = _descriptionEdit.Text;

            CommentAndTitleChanged?.Invoke(_requestContext, _showTitle ? title : "", description);

            Hide();
        }
        
    }
}