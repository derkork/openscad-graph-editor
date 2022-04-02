using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;
using OpenScadGraphEditor.Widgets.InvokableRefactorDialog;
using OpenScadGraphEditor.Widgets.ScadNodeList;
using OpenScadGraphEditor.Widgets.VariableRefactorDialog;

namespace OpenScadGraphEditor
{
    [UsedImplicitly]
    public class GraphEditor : Control
    {
        private AddDialog _addDialog;
        private QuickActionsPopup _quickActionsPopup;
        private Control _editingInterface;
        private TextEdit _textEdit;
        private FileDialog _fileDialog;
        private ScadNodeList _modulesList;
        private ScadNodeList _functionsList;
        private ScadNodeList _variablesList;

        private string _currentFile;

        private bool _dirty;
        private Button _forceSaveButton;
        private Label _fileNameLabel;
        private TabContainer _tabContainer;
        private ScadProject _currentProject;
        private GlobalLibrary _rootResolver;
        private InvokableRefactorDialog _invokableRefactorDialog;
        private VariableRefactorDialog _variableRefactorDialog;
        private bool _codeChanged;

        public override void _Ready()
        {
            OS.LowProcessorUsageMode = true;
            _rootResolver = new GlobalLibrary();

            _tabContainer = this.WithName<TabContainer>("TabContainer");

            _addDialog = this.WithName<AddDialog>("AddDialog");
            _quickActionsPopup = this.WithName<QuickActionsPopup>("QuickActionsPopup");

            _invokableRefactorDialog = this.WithName<InvokableRefactorDialog>("InvokableRefactorDialog");
            _invokableRefactorDialog.RefactoringsRequested += OnRefactoringsRequested;

            _variableRefactorDialog = this.WithName<VariableRefactorDialog>("VariableRefactorDialog");
            _variableRefactorDialog.RefactoringsRequested += OnRefactoringsRequested;

            
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

            this.WithName<Button>("AddModuleButton")
                .Connect("pressed")
                .To(this, nameof(OnAddModulePressed));


            this.WithName<Button>("AddFunctionButton")
                .Connect("pressed")
                .To(this, nameof(OnAddFunctionPressed));

            this.WithName<Button>("AddVariableButton")
                .Connect("pressed")
                .To(this, nameof(OnAddVariablePressed));


            MarkDirty(true);

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
                        new DragData($"Call function {it.Description.Name}",
                            () => NodeFactory.Build<FunctionInvocation>(it.Description))))
            );

            _modulesList.Setup(
                _currentProject.Modules
                    .Select(it => new ScadNodeListEntry(it.Description.Name,
                        () => Open(it),
                        new DragData($"Call module {it.Description.Name}",
                            () => NodeFactory.Build<ModuleInvocation>(it.Description))))
                    .Append(new ScadNodeListEntry(_currentProject.MainModule.Description.Name,
                        () => Open(_currentProject.MainModule)))
            );

            _variablesList.Setup(
                _currentProject.Variables
                    .Select(
                        it => new ScadNodeListEntry(it.Name,
                            () => { },
                            new DragData($"Set {it.Name}", () => NodeFactory.Build<SetVariable>(it))
                        )
                    )
            );
        }

        public void Open(IScadGraph toOpen)
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
            toOpen.Discard();
            _tabContainer.CurrentTab = _tabContainer.GetChildCount() - 1;
            editor.FocusEntryPoint();
        }

        private void OnAddFunctionPressed()
        {
            _invokableRefactorDialog.OpenForNewFunction();
        }

        private void OnAddVariablePressed()
        {
            _variableRefactorDialog.OpenForNewVariable();
        }

        private void OnAddModulePressed()
        {
            _invokableRefactorDialog.OpenForNewModule();
        }

        private void OnRefactoringRequested(Refactoring refactoring)
        {
            var context = new RefactoringContext(this, _currentProject);
            context.AddRefactoring(refactoring);
            context.PerformRefactorings();
            RefreshLists();
            MarkDirty(true);
        }

        private void OnRefactoringsRequested(Refactoring[] refactorings)
        {
            var context = new RefactoringContext(this, _currentProject);

            foreach (var refactoring in refactorings)
            {
                context.AddRefactoring(refactoring);
            }
            context.PerformRefactorings();
            RefreshLists();
            MarkDirty(true);
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
            editor.Changed += MarkDirty;
            editor.RefactoringsRequested += OnRefactoringsRequested;
            editor.NodePopupRequested += OnNodePopupRequested;
        }
        
        private void OnNodePopupRequested(ScadGraphEdit editor,  ScadNode node, Vector2 position)
        {
            // build a list of quick actions that include all refactorings that would apply to the selected node
            var actions = UserSelectableNodeRefactoring
                .GetApplicable(editor, node)
                .Select(it => new QuickAction(it.Title, () => OnRefactoringRequested(it)));
            
            
            // if the node references some invokable, add an action to open the refactor dialog for this invokable.
            if (node is IReferToAnInvokable iReferToAnInvokable)
            {
                actions = actions.Append(new QuickAction($"Refactor {iReferToAnInvokable.InvokableDescription.Name}", 
                    () => _invokableRefactorDialog.Open(iReferToAnInvokable.InvokableDescription)));
            }
            
            _quickActionsPopup.Open(position, actions);

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

            var savedProject = ResourceLoader.Load<SavedProject>(filename, "",  noCache: true);
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
            MarkDirty(true);
        }


        private void OnPreviewToggled(bool preview)
        {
            _editingInterface.Visible = !preview;
            _textEdit.Visible = preview;
        }


        private void MarkDirty(bool codeChange)
        {
            _dirty = true;
            _codeChanged = codeChange;
            if (_codeChanged)
            {
                _forceSaveButton.Text = "[.!.]";
            }
            else
            {
                _forceSaveButton.Text = "[...]";
            }
        }

        private void SaveChanges()
        {
            if (!_dirty)
            {
                return;
            }

            if (_codeChanged)
            {
                RenderScadOutput();
            }

            if (_currentFile != null)
            {
                // save resource
                var savedProject = _currentProject.Save();
                if (ResourceSaver.Save(_currentFile, savedProject) != Error.Ok)
                {
                    GD.Print("Cannot save project!");
                }
                else
                {
                    GD.Print("Saved project!");
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