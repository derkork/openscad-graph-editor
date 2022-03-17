using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactoring;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;
using OpenScadGraphEditor.Widgets.InvokableRefactorDialog;
using OpenScadGraphEditor.Widgets.ScadNodeList;

namespace OpenScadGraphEditor
{
    [UsedImplicitly]
    public class GraphEditor : Control
    {
        private AddDialog _addDialog;
        private Control _editingInterface;
        private TextEdit _textEdit;
        private FileDialog _fileDialog;
        private ScadNodeList _modulesList;
        private ScadNodeList _functionsList;
        private ScadNodeList _variablesList;
        private ScadNodeList _localVariablesList;

        private string _currentFile;

        private bool _dirty;
        private Button _forceSaveButton;
        private Label _fileNameLabel;
        private TabContainer _tabContainer;
        private ScadProject _currentProject;
        private GlobalLibrary _rootResolver;
        private InvokableRefactorDialog _refactorDialog;

        public override void _Ready()
        {
            OS.LowProcessorUsageMode = true;
            _rootResolver = new GlobalLibrary();
            
            _tabContainer = this.WithName<TabContainer>("TabContainer");

            _addDialog = this.WithName<AddDialog>("AddDialog");
            _refactorDialog = this.WithName<InvokableRefactorDialog>("InvokableRefactorDialog");
            _refactorDialog.Connect(nameof(InvokableRefactorDialog.RefactoringRequested))
                .To(this, nameof(OnRefactoringRequested));
            
            _editingInterface = this.WithName<Control>("EditingInterface");
            _textEdit = this.WithName<TextEdit>("TextEdit");
            _fileDialog = this.WithName<FileDialog>("FileDialog");
            _fileNameLabel = this.WithName<Label>("FileNameLabel");
            _forceSaveButton = this.WithName<Button>("ForceSaveButton");
            _forceSaveButton.Connect("pressed")
                .To(this, nameof(SaveChanges));
            

            _modulesList = this.WithName<ScadNodeList>("ModulesList");
            _functionsList = this.WithName<ScadNodeList>("FunctionsList");
            _variablesList = this.WithName<ScadNodeList>("VariablesList");
            _localVariablesList = this.WithName<ScadNodeList>("LocalVariablesList");


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

            this.WithName<Button>("AddFunctionButton")
                .Connect("pressed")
                .To(this, nameof(OnAddFunctionPressed));
            
            
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


        private void RefreshLists()
        {

            _functionsList.Setup(
                _currentProject.Functions
                    .Select(it => new ScadNodeListEntry(it.Description.Name,
                        () => Open(it), 
                        new DragData($"Call function {it.Description.Name}", it.Description)))
            );
            
            _modulesList.Setup(
                _currentProject.Modules
                    .Select(it => new ScadNodeListEntry(it.Description.Name,
                        () => Open(it), 
                        new DragData($"Call module {it.Description.Name}", it.Description)))
                    .Append(new ScadNodeListEntry(_currentProject.MainModule.Description.Name, () => Open(_currentProject.MainModule)))
            );
            
        }

        private void Open(IScadGraph toOpen)
        {
            // check if it is already open
            for (var i = 0; i < _tabContainer.GetChildCount(); i++)
            {
                if (_tabContainer.GetChild<ScadGraphEdit>(i).Description.Id == toOpen.Description.Id)
                {
                    _tabContainer.CurrentTab = i;
                    return;
                }
            }
            
            // if not, open a new tab
            var editor = Prefabs.New<ScadGraphEdit>();
            AttachTo(editor);
            editor.Name = toOpen.Description.NodeNameOrFallback;
            editor.MoveToNewParent(_tabContainer);
            _currentProject.TransferData(toOpen, editor);
            _tabContainer.CurrentTab = _tabContainer.GetChildCount() - 1;
            editor.FocusEntryPoint();
        }

        private void OnAddFunctionPressed()
        {
            _refactorDialog.OpenForNewFunction();
        }

        private void OnRefactoringRequested(Refactoring.Refactoring refactoring)
        {
            if (refactoring is IntroduceInvokableRefactoring introduceInvokableRefactoring)
            {
                var invokableDescription = introduceInvokableRefactoring.Description;
                var graph = _currentProject.AddInvokable(invokableDescription);
                RefreshLists();
                Open(graph);
            }
            
        }
        
        private void OnNewButtonPressed()
        {
            Clear();
            _currentProject = new ScadProject(_rootResolver);
            Open(_currentProject.MainModule);
            RefreshLists();
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

            _currentFile = filename;
            _fileNameLabel.Text = filename;

            _currentProject = new ScadProject(_rootResolver);
            _currentProject.Load(savedProject);

            Open(_currentProject.MainModule);
            RefreshLists();
           
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