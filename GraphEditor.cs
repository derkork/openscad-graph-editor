using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor
{
    [UsedImplicitly]
    public class GraphEditor : Control
    {
        private AddDialog _addDialog;
        private Control _editingInterface;
        private TextEdit _textEdit;
        private FileDialog _fileDialog;

        private string _currentFile;

        private bool _dirty;
        private Button _forceSaveButton;
        private Label _fileNameLabel;
        private TabContainer _tabContainer;
        private ScadProjectContext _currentProject;

        public override void _Ready()
        {
            OS.LowProcessorUsageMode = true;
            
            _tabContainer = this.WithName<TabContainer>("TabContainer");

            _addDialog = this.WithName<AddDialog>("AddDialog");
            _editingInterface = this.WithName<Control>("EditingInterface");
            _textEdit = this.WithName<TextEdit>("TextEdit");
            _fileDialog = this.WithName<FileDialog>("FileDialog");
            _fileNameLabel = this.WithName<Label>("FileNameLabel");
            _forceSaveButton = this.WithName<Button>("ForceSaveButton");
            _forceSaveButton.Connect("pressed")
                .To(this, nameof(SaveChanges));



            this.WithName<Button>("NewButton")
                .Connect("pressed")
                .To(this, nameof(OnNewButtonPressed));
            
            this.WithName<Button>("PreviewButton")
                .Connect("toggled")
                .To(this, nameof(OnPreviewToggled));

            this.WithName<Button>("OpenButton")
                .Connect("pressed")
                .To(this, nameof(OnOpenFilePressed));
            
            this.WithName<Button>("SaveAsButton")
                .Connect("pressed")
                .To(this, nameof(OnSaveAsPressed));
            
            
            MarkDirty();

            this.WithName<Timer>("Timer")
                .Connect("timeout")
                .To(this, nameof(SaveChanges));
            
            OnNewButtonPressed();
        }

        private void Clear()
        {
            _currentProject?.Discard();
            _currentFile = null;
            _fileNameLabel.Text = "<not yet saved to a file>";
        }

        private void OnNewButtonPressed()
        {
            Clear();
            _currentProject = new ScadProjectContext();
            var editor = Prefabs.New<ScadGraphEdit>();
            AttachTo(editor);
            editor.MoveToNewParent(_tabContainer);
            _currentProject.MainModule.TransferTo(editor);
        }

        private void AttachTo(ScadGraphEdit editor)
        {
            editor.Setup(_addDialog);
            editor.Connect(nameof(ScadGraphEdit.NeedsUpdate))
                .To(this, nameof(MarkDirty));
        }

        private void OnOpenFilePressed()
        {
            _fileDialog.Mode = FileDialog.ModeEnum.OpenFile;
            _fileDialog.PopupCentered();
            _fileDialog.Connect("file_selected")
                .WithFlags(ConnectFlags.Oneshot)
                .To(this, nameof(OnOpenFile));
        }

        private void OnOpenFile(string filename)
        {
            var file = new File();
            if (!file.FileExists(filename))
            {
                return;
            }

            var savedProject = ResourceLoader.Load<SavedProject>(filename, noCache: true);
            if (savedProject == null)
            {
                GD.Print("Cannot load file!");
                return;
            }

            Clear();

            _currentProject = new ScadProjectContext();
            _currentProject.Load(savedProject);
            
            var editor = Prefabs.New<ScadGraphEdit>();
            editor.MoveToNewParent(_tabContainer);
            AttachTo(editor);
            _currentProject.MainModule.TransferTo(editor);
            _currentFile = filename;
            _fileNameLabel.Text = _currentFile;
            
            RenderScadOutput();
        }
        
        private void OnSaveAsPressed()
        {
            _fileDialog.Mode = FileDialog.ModeEnum.SaveFile;
            _fileDialog.PopupCentered();
            _fileDialog.Connect("file_selected")
                .WithFlags(ConnectFlags.Oneshot)
                .To(this, nameof(OnSaveFileSelected));
        }

        private void OnSaveFileSelected(string filename)
        {
            _currentFile = filename;
            _fileNameLabel.Text = filename;
            MarkDirty();
        }
        


        private void OnPreviewToggled(bool preview)
        {
            _editingInterface.Visible = !preview;
            _textEdit.Visible = preview;
        }
        

        private void MarkDirty()
        {
            _dirty = true;
            _forceSaveButton.Text = "[...]";
        }

        private void SaveChanges()
        {
            if (!_dirty)
            {
                return;
            }

            RenderScadOutput();

            if (_currentFile != null)
            {
                // save resource
                var savedProject = _currentProject.Save();
                if (ResourceSaver.Save(_currentFile, savedProject) != Error.Ok)
                {
                    GD.Print("Cannot save project!");
                }
            }

            _forceSaveButton.Text = "[OK]";
            _dirty = false;

        }

        private void RenderScadOutput()
        {
            var rendered = _currentProject.Render();
            _textEdit.Text = rendered;

            if (_currentFile != null)
            {
                // save rendered output
                var file = new File();
                if (file.Open(_currentFile + ".scad", File.ModeFlags.Write) == Error.Ok)
                {
                    file.StoreString(rendered);
                    file.Close();
                }
                else
                {
                    GD.Print("Cannot save SCAD!");
                }
            }
        }



    }
}