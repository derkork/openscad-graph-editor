using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Widgets.SettingsDialog
{
    public class SettingsDialog : WindowDialog
    {
        private OptionButton _editorScaleOptionButton;
        private Configuration _configuration;

        public override void _Ready()
        {
            _editorScaleOptionButton = this.WithName<OptionButton>("EditorScale");
            
            this.WithName<Button>("OKButton")
                .Connect("pressed")
                .To(this, nameof(OnOkPressed));
            
            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelPressed));
        }

        public void Open(Configuration configuration)
        {
            _configuration = configuration;
            var editorScale = configuration.GetEditorScalePercent();
            _editorScaleOptionButton.Selected = editorScale switch
            {
                150 => 1,
                200 => 2,
                _ => 0
            };
            
            SetAsMinsize();
            PopupCentered();
        }
        
        
        public void OnOkPressed()
        {
            var editorScale = _editorScaleOptionButton.Selected switch
            {
                1 => 150,
                2 => 200,
                _ => 100
            };
            _configuration.SetEditorScalePercent(editorScale);
            Hide();
        }
        
        public void OnCancelPressed()
        {
            Hide();
        }
    }
}