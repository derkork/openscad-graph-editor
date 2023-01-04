using System;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.SettingsDialog
{
    [UsedImplicitly]
    public class SettingsDialog : WindowDialog
    {
        private Configuration _configuration;
        private ScadProject _currentProject;

        
        private TextEdit _projectPreambleTextEdit;

        private OptionButton _editorScaleOptionButton;
        private FileSelectBox.FileSelectBox _fileSelectBox;
        private TextEdit _defaultPreambleTextEdit;
        private SpinBox _backupsSpinBox;
        
        public event Action<string, Refactoring> RefactoringRequested; 

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

        public void Open(Configuration configuration, ScadProject currentProject)
        {
            _configuration = configuration;
            _currentProject = currentProject;
            var editorScale = configuration.GetEditorScalePercent();
            _editorScaleOptionButton.Selected = editorScale switch
            {
                150 => 1,
                200 => 2,
                _ => 0
            };

            _fileSelectBox.CurrentPath = _configuration.GetOpenScadPath();
            _backupsSpinBox.Value = _configuration.GetNumberOfBackups();
            _defaultPreambleTextEdit.Text = _configuration.GetDefaultPreamble();
            _projectPreambleTextEdit.Text = _currentProject.Preamble;
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
            _configuration.SetEditorScalePercent(editorScale);
            _configuration.SetOpenScadPath(_fileSelectBox.CurrentPath);
            _configuration.SetNumberOfBackups((int)_backupsSpinBox.Value);
            _configuration.SetDefaultPreamble(_defaultPreambleTextEdit.Text);
            if (_projectPreambleTextEdit.Text != _currentProject.Preamble)
            {
                RefactoringRequested?.Invoke("Update project preamble", new ChangeProjectPreambleRefactoring(_projectPreambleTextEdit.Text));
            }
            Hide();
        }
        
        public void OnCancelPressed()
        {
            Hide();
        }
    }
}