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
        public event Action<RequestContext, string, string> CommentChanged;
        
        private LineEdit _titleEdit;
        private TextEdit _descriptionEdit;
        private RequestContext _requestContext;

        public override void _Ready()
        {
            _titleEdit = this.WithName<LineEdit>("TitleEdit");
            _descriptionEdit = this.WithName<TextEdit>("DescriptionEdit");
            
            // connect cancel button
            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelButtonPressed));
            
            // connect OK button
            this.WithName<Button>("OKButton")
                .Connect("pressed")
                .To(this, nameof(OnOkButtonPressed));
            
        }

        public void Open(RequestContext requestContext, string title = "", string description = "")
        {
            _requestContext = requestContext;
            _titleEdit.Text = title;
            _descriptionEdit.Text = description;
            
            PopupCenteredMinsize();
            
            _titleEdit.GrabFocus();
        }
        

        private void OnCancelButtonPressed()
        {
            Hide();
        }
        
        private void OnOkButtonPressed()
        {
            var title = _titleEdit.Text;
            var description = _descriptionEdit.Text;

            CommentChanged?.Invoke(_requestContext, title, description);
            Hide();
        }
        
    }
}