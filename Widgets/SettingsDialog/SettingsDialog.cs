using Godot;
using GodotExt;
using OpenScadGraphEditor.Library.External;

namespace OpenScadGraphEditor.Widgets.SettingsDialog
{
    public class SettingsDialog : WindowDialog
    {
        private OptionButton _editorScaleOptionButton;
        private Configuration _configuration;
        private FileSelectBox.FileSelectBox _fileSelectBox;

        public override void _Ready()
        {
            _editorScaleOptionButton = this.WithName<OptionButton>("EditorScale");
            
            _fileSelectBox = this.WithName<FileSelectBox.FileSelectBox>("FileSelectBox");
            _fileSelectBox.Filters = new[] {$"{PathResolver.GetOpenScadExecutableName()};OpenSCAD"};
            _fileSelectBox.OnSelectPressed += () => _fileSelectBox.OpenSelectionDialog();
            
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

            _fileSelectBox.CurrentPath = _configuration.GetOpenScadPath();
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
            _configuration.SetOpenScadPath(_fileSelectBox.CurrentPath);
            Hide();
        }
        
        public void OnCancelPressed()
        {
            Hide();
        }
    }
}