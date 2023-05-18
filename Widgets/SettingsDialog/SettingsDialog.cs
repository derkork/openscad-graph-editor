using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.SettingsDialog
{
    [UsedImplicitly]
    public class SettingsDialog : WindowDialog
    {
        private TextEdit _projectPreambleTextEdit;
        private OptionButton _editorScaleOptionButton;
        private FileSelectBox.FileSelectBox _fileSelectBox;
        private TextEdit _defaultPreambleTextEdit;
        private SpinBox _backupsSpinBox;
        private IEditorContext _context;

        public override void _Ready()
        {
            this.WithName<TabContainer>("SettingsTabContainer")
                .Connect("tab_changed")
                .To(this, nameof(OnTabChanged));
            _projectPreambleTextEdit = this.WithName<TextEdit>("ProjectPreambleTextEdit");
            _editorScaleOptionButton = this.WithName<OptionButton>("EditorScale");
            
            _fileSelectBox = this.WithName<FileSelectBox.FileSelectBox>("FileSelectBox");
            _fileSelectBox.Filters = new[] {$"{PathResolver.GetOpenScadExecutableName()};OpenSCAD"};
            _fileSelectBox.OnSelectPressed += () => _fileSelectBox.OpenSelectionDialog();
            _backupsSpinBox = this.WithName<SpinBox>("BackupsSpinBox");
            _defaultPreambleTextEdit = this.WithName<TextEdit>("DefaultPreambleTextEdit");
            
            this.WithName<Button>("OKButton")
                .Connect("pressed")
                .To(this, nameof(OnOkPressed));
            
            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelPressed));
        }

        public void Open(IEditorContext context)
        {
            _context = context;
            var configuration = context.Configuration;
            
            var editorScale = configuration.GetEditorScalePercent();
            _editorScaleOptionButton.Selected = editorScale switch
            {
                150 => 1,
                200 => 2,
                _ => 0
            };

            _fileSelectBox.CurrentPath = configuration.GetOpenScadPath();
            _backupsSpinBox.Value = configuration.GetNumberOfBackups();
            _defaultPreambleTextEdit.Text = configuration.GetDefaultPreamble();
            _projectPreambleTextEdit.Text = _context.CurrentProject.Preamble;
            PopupCentered();
            SetAsMinsize();
        }
        
        private void OnTabChanged([UsedImplicitly] int _)
        {
            SetAsMinsize();
        }
        
        public void OnOkPressed()
        {
            var editorScale = _editorScaleOptionButton.Selected switch
            {
                1 => 150,
                2 => 200,
                _ => 100
            };
            _context.Configuration.SetEditorScalePercent(editorScale);
            _context.Configuration.SetOpenScadPath(_fileSelectBox.CurrentPath);
            _context.Configuration.SetNumberOfBackups((int)_backupsSpinBox.Value);
            _context.Configuration.SetDefaultPreamble(_defaultPreambleTextEdit.Text);
            if (_projectPreambleTextEdit.Text != _context.CurrentProject.Preamble)
            {
                _context.PerformRefactoring("Update project preamble", new ChangeProjectPreambleRefactoring(_projectPreambleTextEdit.Text));
            }
            Hide();
        }
        
        public void OnCancelPressed()
        {
            Hide();
        }
    }
}