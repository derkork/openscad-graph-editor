using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.History;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;
using OpenScadGraphEditor.Widgets.IconButton;
using OpenScadGraphEditor.Widgets.ImportDialog;
using OpenScadGraphEditor.Widgets.InvokableRefactorDialog;
using OpenScadGraphEditor.Widgets.ProjectTree;
using OpenScadGraphEditor.Widgets.VariableRefactorDialog;

namespace OpenScadGraphEditor
{
    [UsedImplicitly]
    public class GraphEditor : Control
    {
        // TODO: this class gets _really_ big.

        private AddDialog _addDialog;
        private QuickActionsPopup _quickActionsPopup;
        private Control _editingInterface;
        private TextEdit _textEdit;
        private FileDialog _fileDialog;
        private ProjectTree _projectTree;
        private ImportDialog _importDialog;
        private IconButton _undoButton;
        private IconButton _redoButton;

        private string _currentFile;

        private bool _dirty;
        private Button _forceSaveButton;
        private Label _fileNameLabel;
        private TabContainer _tabContainer;
        private ScadProject _currentProject;
        private HistoryStack _currentHistoryStack;
        private GlobalLibrary _rootResolver;
        private InvokableRefactorDialog _invokableRefactorDialog;
        private VariableRefactorDialog _variableRefactorDialog;
        private readonly List<IAddDialogEntry> _addDialogEntries = new List<IAddDialogEntry>();
        private LightWeightGraph _copyBuffer;

        private bool _codeChanged;

        public override void _Ready()
        {
            OS.LowProcessorUsageMode = true;
            _rootResolver = new GlobalLibrary();

            _tabContainer = this.WithName<TabContainer>("TabContainer");

            _addDialog = this.WithName<AddDialog>("AddDialog");
            _quickActionsPopup = this.WithName<QuickActionsPopup>("QuickActionsPopup");
            _importDialog = this.WithName<ImportDialog>("ImportDialog");
            _importDialog.OnNewImportRequested += OnNewImportRequested;

            _invokableRefactorDialog = this.WithName<InvokableRefactorDialog>("InvokableRefactorDialog");
            _invokableRefactorDialog.InvokableModificationRequested +=
                (description, refactorings) => PerformRefactorings($"Change {description.Name}", refactorings);
            _invokableRefactorDialog.InvokableCreationRequested +=
                // create the invokable, then open it's graph in a new tab.
                (description, refactorings) => PerformRefactorings($"Create {description.Name}", refactorings,
                    () => Open(_currentProject.FindDefiningGraph(description)));

            _variableRefactorDialog = this.WithName<VariableRefactorDialog>("VariableRefactorDialog");
            _variableRefactorDialog.RefactoringsRequested +=
                (refactorings) => PerformRefactorings("Rename variable", refactorings);

            _editingInterface = this.WithName<Control>("EditingInterface");
            _textEdit = this.WithName<TextEdit>("TextEdit");
            _fileDialog = this.WithName<FileDialog>("FileDialog");
            _fileNameLabel = this.WithName<Label>("FileNameLabel");
            _forceSaveButton = this.WithName<Button>("ForceSaveButton");
            _forceSaveButton.Connect("pressed")
                .To(this, nameof(SaveChanges));


            _projectTree = this.WithName<ProjectTree>("ProjectTree");

            _projectTree.ItemActivated += Open;
            _projectTree.ItemContextMenuRequested += OnItemContextMenuRequested;

            _undoButton = this.WithName<IconButton>("UndoButton");
            _undoButton.ButtonPressed += Undo;

            _redoButton = this.WithName<IconButton>("RedoButton");
            _redoButton.ButtonPressed += Redo;

            this.WithName<IconButton>("AddExternalReferenceButton")
                .ButtonPressed += OnImportScadFile;

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

            this.WithName<IconButton>("AddModuleButton")
                .ButtonPressed += OnAddModulePressed;

            this.WithName<IconButton>("AddFunctionButton")
                .ButtonPressed += OnAddFunctionPressed;

            this.WithName<IconButton>("AddVariableButton")
                .ButtonPressed += OnAddVariablePressed;

            MarkDirty(true);

            this.WithName<Timer>("Timer")
                .Connect("timeout")
                .To(this, nameof(SaveChanges));

            _copyBuffer = new LightWeightGraph();

            OnNewButtonPressed();
        }

        private void OnNewImportRequested(string path, IncludeMode includeMode)
        {
            OnRefactoringRequested($"Add reference to {path}", new AddExternalReferenceRefactoring(path, includeMode));
        }

        private void OnItemContextMenuRequested(ProjectTreeEntry entry, Vector2 mousePosition)
        {
            var actions = new List<QuickAction>();
            var title = $"Delete {entry.Title}";
            if (entry is ScadInvokableTreeEntry invokableTreeEntry)
            {
                switch (invokableTreeEntry.Description)
                {
                    case FunctionDescription functionDescription:
                        if (_currentProject.IsDefinedInThisProject(functionDescription))
                        {
                            actions.Add(new QuickAction(title,
                                () => OnRefactoringRequested(title,
                                    new DeleteInvokableRefactoring(functionDescription))));
                        }

                        break;
                    case ModuleDescription moduleDescription:
                        if (_currentProject.IsDefinedInThisProject(moduleDescription))
                        {
                            actions.Add(new QuickAction(title,
                                () => OnRefactoringRequested(title,
                                    new DeleteInvokableRefactoring(moduleDescription))));
                        }

                        break;
                }
            }

            if (entry is ScadVariableTreeEntry scadVariableListEntry)
            {
                if (_currentProject.IsDefinedInThisProject(scadVariableListEntry.Description))
                {
                    actions.Add(new QuickAction(title,
                        () => OnRefactoringRequested(
                            title, new DeleteVariableRefactoring(scadVariableListEntry.Description))));
                }
            }

            if (entry is ExternalReferenceTreeEntry externalReferenceTreeEntry &&
                !externalReferenceTreeEntry.Description.IsTransitive)
            {
                var removeReferenceTitle = $"Remove reference to {entry.Title}";
                actions.Add(new QuickAction(removeReferenceTitle,
                    () => OnRefactoringRequested(
                        removeReferenceTitle,
                        new DeleteExternalReferenceRefactoring(externalReferenceTreeEntry.Description))));
            }

            _quickActionsPopup.Open(mousePosition, actions);
        }

        private void Open(ProjectTreeEntry entry)
        {
            if (entry is ScadInvokableTreeEntry invokableTreeEntry
                && _currentProject.IsDefinedInThisProject(invokableTreeEntry.Description))
            {
                Open(_currentProject.FindDefiningGraph(invokableTreeEntry.Description));
            }
        }

        private void Clear()
        {
            _currentProject?.Discard();
            _currentHistoryStack = null;
            _currentFile = null;
            _fileNameLabel.Text = "<not yet saved to a file>";
        }


        private void RefreshControls()
        {
            _undoButton.Disabled = !_currentHistoryStack.CanUndo(out var undoDescription);
            _undoButton.HintTooltip = $"Undo : {undoDescription}";

            _redoButton.Disabled = !_currentHistoryStack.CanRedo(out var redoDescription);
            _redoButton.HintTooltip = $"Redo : {redoDescription}";

            _projectTree.Setup(new List<ProjectTreeEntry>() {new RootProjectTreeEntry(_currentProject)});


            // Fill the Add Dialog Entries.

            _addDialogEntries.Clear();

            _addDialogEntries.AddRange(
                BuiltIns.LanguageLevelNodes
                    .Select(it => new NodeBasedEntry(
                        Resources.ScadBuiltinIcon,
                        () => NodeFactory.Build(it),
                        AddNode
                    ))
            );

            _addDialogEntries.AddRange(
                BuiltIns.Functions
                    .Select(it => new NodeBasedEntry(
                        Resources.FunctionIcon,
                        () => NodeFactory.Build<FunctionInvocation>(it),
                        AddNode
                    ))
            );

            _addDialogEntries.AddRange(
                BuiltIns.Modules
                    .Select(it => new NodeBasedEntry(
                        Resources.ModuleIcon,
                        () => NodeFactory.Build<ModuleInvocation>(it),
                        AddNode
                    )));


            // also add entries for functions and modules defined in the project
            _addDialogEntries.AddRange(
                _currentProject.Functions
                    .Select(it => new NodeBasedEntry(
                        Resources.FunctionIcon,
                        () => NodeFactory.Build<FunctionInvocation>(it.Description),
                        AddNode
                    ))
            );

            _addDialogEntries.AddRange(
                _currentProject.Modules
                    .Select(it => new NodeBasedEntry(
                        Resources.ModuleIcon,
                        () => NodeFactory.Build<ModuleInvocation>(it.Description),
                        AddNode
                    ))
            );

            // add getter and setters for all variables
            _addDialogEntries.AddRange(
                _currentProject.Variables
                    .Select(it => new NodeBasedEntry(
                        Resources.VariableIcon,
                        () => NodeFactory.Build<SetVariable>(it),
                        AddNode
                    ))
            );

            _addDialogEntries.AddRange(
                _currentProject.Variables
                    .Select(it => new NodeBasedEntry(
                        Resources.VariableIcon,
                        () => NodeFactory.Build<GetVariable>(it),
                        AddNode
                    ))
            );


            // add call entries for all externally referenced functions and modules
            foreach (var reference in _currentProject.ExternalReferences)
            {
                _addDialogEntries.AddRange(
                    reference.Functions
                        .Select(it => new NodeBasedEntry(
                                Resources.FunctionIcon,
                                () => NodeFactory.Build<FunctionInvocation>(it),
                                AddNode
                            )
                        )
                );

                _addDialogEntries.AddRange(
                    reference.Modules
                        .Select(it => new NodeBasedEntry(
                                Resources.ModuleIcon,
                                () => NodeFactory.Build<ModuleInvocation>(it),
                                AddNode
                            )
                        )
                );
            }
        }

        private void Undo()
        {
            GdAssert.That(_currentHistoryStack.CanUndo(out _), "Cannot undo");
            var editorState = _currentHistoryStack.Undo(_currentProject);
            RestoreEditorState(editorState);
            MarkDirty(true);
        }

        private void Redo()
        {
            GdAssert.That(_currentHistoryStack.CanRedo(out _), "Cannot redo");
            var editorState = _currentHistoryStack.Redo(_currentProject);
            RestoreEditorState(editorState);
            MarkDirty(true);
        }


        private void OnImportScadFile()
        {
            _importDialog.OpenForNewImport(_currentFile);
        }


        private void AddNode(RequestContext context, NodeBasedEntry entry)
        {
            AddNode(context, entry.CreateNode());
        }

        private void AddNode(RequestContext context, ScadNode node)
        {
            node.Offset = context.LastReleasePosition;
            var otherNode = context.SourceNode ?? context.DestinationNode;
            var isIncoming = context.SourceNode != null;
            var otherPort = context.LastPort;

            PerformRefactorings("Add node",
                new AddNodeRefactoring(context.Source, node, otherNode, otherPort, isIncoming));
        }

        private ScadGraphEdit Open(IScadGraph toOpen)
        {
            // check if it is already open
            for (var i = 0; i < _tabContainer.GetChildCount(); i++)
            {
                var existingEditor = _tabContainer.GetChild<ScadGraphEdit>(i);
                if (existingEditor.Description.Id == toOpen.Description.Id)
                {
                    _tabContainer.CurrentTab = i;
                    return existingEditor;
                }
            }

            // if not, open a new tab
            var editor = Prefabs.New<ScadGraphEdit>();
            editor.RefactoringsRequested += PerformRefactorings;
            editor.NodePopupRequested += OnNodePopupRequested;
            editor.ItemDataDropped += OnItemDataDropped;
            editor.AddDialogRequested += OnAddDialogRequested;
            editor.CopyRequested += OnCopyRequested;
            editor.PasteRequested += OnPasteRequested;
            editor.CutRequested += OnCutRequested;

            editor.Name = toOpen.Description.NodeNameOrFallback;
            editor.MoveToNewParent(_tabContainer);
            _currentProject.TransferData(toOpen, editor);
            toOpen.Discard();
            _tabContainer.CurrentTab = _tabContainer.GetChildCount() - 1;
            editor.FocusEntryPoint();
            return editor;
        }

        private void OnCutRequested(ScadGraphEdit source, List<ScadNode> selection)
        {
            // first copy the nodes
            OnCopyRequested(source, selection);

            // then run refactorings to delete them
            var deleteRefactorings = selection
                // only delete nodes which can be deleted
                .Where(it => !(it is ICannotBeDeleted))
                // will implicitly also delete the connections.
                .Select(it => new DeleteNodeRefactoring(source, it))
                .ToList();

            PerformRefactorings("Cut nodes", deleteRefactorings);
        }

        /// <summary>
        /// Copies the given nodes from the source graph into a copy buffer.
        /// </summary>
        private LightWeightGraph MakeCopyBuffer(IScadGraph source, IEnumerable<ScadNode> selection)
        {
            var result = new LightWeightGraph();
            // remove all nodes which cannot be deleted (which are usually nodes that are built in so they cannot be copied either).
            // also make it a HashSet so we don't have duplicates and have quicker lookups
            var sanitizedSet = selection
                .Where(it => !(it is ICannotBeDeleted))
                .ToHashSet();

            var idMapping = new Dictionary<string, string>();
            // make copies of all the nodes and put them into the copy buffer
            foreach (var node in sanitizedSet)
            {
                var copy = NodeFactory.Duplicate(node, _currentProject);
                // make note of the id mapping, because we need this to resolve connections
                // later
                idMapping[node.Id] = copy.Id;
                result.AddNode(copy);
            }

            //  copy all connections which are between the selected nodes
            foreach (var connection in source.GetAllConnections())
            {
                if (sanitizedSet.Contains(connection.From) && sanitizedSet.Contains(connection.To))
                {
                    result.AddConnection(idMapping[connection.From.Id], connection.FromPort,
                        idMapping[connection.To.Id], connection.ToPort);
                }
            }

            return result;
        }

        private void OnCopyRequested(ScadGraphEdit source, List<ScadNode> selection)
        {
            _copyBuffer = MakeCopyBuffer(source, selection);
        }

        private void OnPasteRequested(ScadGraphEdit target, Vector2 position)
        {
            // now make another copy from the copy buffer and paste that. The reason we do this is because
            // nodes need unique Ids so for each pasting we need to make a new copy.
            var copy = MakeCopyBuffer(_copyBuffer, _copyBuffer.GetAllNodes());

            // now the clipboard may contain nodes that are not allowed in the given target graph. So we need
            // to filter these out here and also delete all connections to these nodes.
            var disallowedNodes = copy.GetAllNodes()
                .Where(it => !target.Description.CanUse(it))
                .ToList();

            // delete all connections to the disallowed nodes
            copy.GetAllConnections()
                .Where(it => disallowedNodes.Any(it.InvolvesNode))
                .ToList() // avoid concurrent modification
                .ForAll(it => copy.RemoveConnection(it));

            // and the nodes themselves
            disallowedNodes.ForAll(it => copy.RemoveNode(it));

            var scadNodes = copy.GetAllNodes().ToList();
            if (scadNodes.Count == 0)
            {
                return; // nothing to paste
            }

            // we now need to normalize the position of the nodes so they are pasted in the correct position
            // we do this by finding the bounding box of the nodes and then offsetting them by the difference between the position
            // of the bounding box and the position of the node that is closes to top left

            // we start with a rectangle that is a point that simply has the position of the first node
            var boundingBox = new Rect2(scadNodes[0].Offset, Vector2.Zero);
            // now expand this rectangle so it contains all the points of all the offsets of the nodes
            boundingBox = scadNodes.Aggregate(boundingBox, (current, node) => current.Expand(node.Offset));
            // now we calculate the offset of the bounding box position and the desired position
            var offset = position - boundingBox.Position;

            // and offset every node by this
            scadNodes.ForAll(it => it.Offset += offset);

            // now run the refactorings to add the given nodes and connections
            var refactorings = new List<Refactoring>();
            foreach (var node in scadNodes)
            {
                refactorings.Add(new AddNodeRefactoring(target, node));
            }

            foreach (var connection in copy.GetAllConnections())
            {
                refactorings.Add(
                    new AddConnectionRefactoring(
                        new ScadConnection(target, connection.From, connection.FromPort, connection.To,
                            connection.ToPort
                        )
                    )
                );
            }

            PerformRefactorings("Paste nodes", refactorings, () => target.SelectNodes(scadNodes));
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

        private void OnRefactoringRequested(string humanReadableDescription, Refactoring refactoring)
        {
            PerformRefactorings(humanReadableDescription, refactoring);
        }

        private void PerformRefactorings(string description, params Refactoring[] refactorings)
        {
            PerformRefactorings(description, (IEnumerable<Refactoring>) refactorings);
        }

        private void PerformRefactorings(string description, IEnumerable<Refactoring> refactorings,
            params Action[] after)
        {
            var context = new RefactoringContext(_currentProject);
            context.PerformRefactorings(refactorings, after);

            // in case a module or function was deleted the current tab may be off
            var currentTabId = _tabContainer.CurrentTab;
            var childCount = _tabContainer.GetChildCount<ScadGraphEdit>();
            if (currentTabId >= childCount)
            {
                _tabContainer.CurrentTab = childCount - 1;
            }

            // important, the snapshot must be made _after_ the changes.
            _currentHistoryStack.AddSnapshot(description, _currentProject, GetEditorState());

            RefreshControls();
            MarkDirty(true);
        }

        private void OnNewButtonPressed()
        {
            Clear();
            _currentProject = new ScadProject(_rootResolver);
            Open(_currentProject.MainModule);
            // important, this needs to be done after the main module is opened
            _currentHistoryStack = new HistoryStack(_currentProject, GetEditorState());
            RefreshControls();
        }

        private EditorState GetEditorState()
        {
            // create a list of currently open tabs
            var tabs =
                _tabContainer.GetChildNodes<ScadGraphEdit>()
                    .Select((it, idx) =>
                        new EditorOpenTab(it.Description.Id, it.ScrollOffset));
            return new EditorState(tabs, _tabContainer.CurrentTab);
        }

        private void RestoreEditorState(EditorState editorState)
        {
            // close all open tabs
            foreach (var tab in _tabContainer.GetChildNodes<ScadGraphEdit>())
            {
                tab.Discard();
            }

            // open all tabs that were open back then
            foreach (var openTab in editorState.OpenTabs)
            {
                var invokableId = openTab.InvokableId;
                var invokable = _currentProject.AllDeclaredInvokables.First(it => it.Description.Id == invokableId);
                var editor = Open(invokable);
                editor.ScrollOffset = openTab.ScrollOffset;
            }

            // and finally select the tab that was selected
            _tabContainer.CurrentTab = editorState.CurrentTabIndex;

            RefreshControls();
        }


        private void OnAddDialogRequested(RequestContext context)
        {
            // when shift is pressed this means we want to have a reroute node.
            if (Input.IsKeyPressed((int) KeyList.Shift))
            {
                AddNode(context, NodeFactory.Build<RerouteNode>());
                return;
            }

            // otherwise let the user choose a node.
            _addDialog.Open(_addDialogEntries, context);
        }

        private void OnItemDataDropped(ScadGraphEdit graph, ProjectTreeEntry entry, Vector2 mousePosition,
            Vector2 virtualPosition)
        {
            // did we drag an invokable from the list to the graph?
            if (entry is ScadInvokableTreeEntry invokableListEntry)
            {
                // then add a new node calling the invokable.
                var description = invokableListEntry.Description;
                ScadNode node;
                if (description is FunctionDescription functionDescription)
                {
                    node = NodeFactory.Build<FunctionInvocation>(functionDescription);
                }
                else if (description is ModuleDescription moduleDescription)
                {
                    node = NodeFactory.Build<ModuleInvocation>(moduleDescription);
                }
                else
                {
                    throw new InvalidOperationException("Unknown invokable type.");
                }

                node.Offset = virtualPosition;
                OnRefactoringRequested("Add node", new AddNodeRefactoring(graph, node));
            }

            // did we drag a variable from the list to the graph?
            if (entry is ScadVariableTreeEntry variableListEntry)
            {
                // in case of a variable we can either _get_ or _set_ the variable
                // so we will need to open a popup menu to let the user choose.
                var getNode = NodeFactory.Build<GetVariable>(variableListEntry.Description);
                getNode.Offset = virtualPosition;
                var setNode = NodeFactory.Build<SetVariable>(variableListEntry.Description);
                setNode.Offset = virtualPosition;

                var actions = new List<QuickAction>
                {
                    new QuickAction($"Get {variableListEntry.Description.Name}",
                        () => OnRefactoringRequested("Add node", new AddNodeRefactoring(graph, getNode))),
                    new QuickAction($"Set {variableListEntry.Description.Name}",
                        () => OnRefactoringRequested("Add node", new AddNodeRefactoring(graph, setNode)))
                };

                _quickActionsPopup.Open(mousePosition, actions);
            }
        }

        private void OnNodePopupRequested(ScadGraphEdit editor, ScadNode node, Vector2 position)
        {
            // build a list of quick actions that include all refactorings that would apply to the selected node
            var actions = UserSelectableNodeRefactoring
                .GetApplicable(editor, node)
                .Select(it => new QuickAction(it.Title, () => OnRefactoringRequested(it.Title, it)));


            // if the node references some invokable, add an action to open the refactor dialog for this invokable.
            if (node is IReferToAnInvokable iReferToAnInvokable)
            {
                var name = iReferToAnInvokable.InvokableDescription.Name;
                // if the node isn't actually the entry point, add an action to go to the entrypoint
                if (!(node is EntryPoint))
                {
                    actions = actions.Append(
                        new QuickAction($"Go to definition of {name}",
                            () => Open(_currentProject.FindDefiningGraph(iReferToAnInvokable.InvokableDescription))
                        )
                    );
                }

                actions = actions.Append(
                    new QuickAction($"Refactor {name}",
                        () => _invokableRefactorDialog.Open(iReferToAnInvokable.InvokableDescription)
                    )
                );
            }

            if (node is ICanHaveModifier)
            {
                var currentModifiers = node.GetModifiers();

                var hasDebug = currentModifiers.HasFlag(ScadNodeModifier.Debug);
                actions = actions.Append(
                    new QuickAction("Debug subtree",
                        () => OnRefactoringRequested("Debug subtree",
                            new ToggleModifierRefactoring(editor, node, ScadNodeModifier.Debug, !hasDebug)), true, hasDebug));
                
                var hasRoot = currentModifiers.HasFlag(ScadNodeModifier.Root);
                actions = actions.Append(
                    new QuickAction("Make node root",
                        () => OnRefactoringRequested("Make node root",
                            new ToggleModifierRefactoring(editor, node, ScadNodeModifier.Root, !hasRoot)), true, hasRoot));

                
                var hasBackground = currentModifiers.HasFlag(ScadNodeModifier.Background);
                actions = actions.Append(
                    new QuickAction("Background subtree",
                        () => OnRefactoringRequested("Background subtree",
                            new ToggleModifierRefactoring(editor, node, ScadNodeModifier.Background, !hasBackground)), true, hasBackground));

                var hasDisable = currentModifiers.HasFlag(ScadNodeModifier.Disable);
                actions = actions.Append(
                    new QuickAction("Disable subtree",
                        () => OnRefactoringRequested("Disable subtree",
                            new ToggleModifierRefactoring(editor, node, ScadNodeModifier.Disable, !hasDisable)), true, hasDisable));

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

            var savedProject = ResourceLoader.Load<SavedProject>(filename, "", noCache: true);
            if (savedProject == null)
            {
                GD.Print("Cannot load file!");
                return;
            }

            Clear();

            _currentFile = filename;
            _fileNameLabel.Text = filename;

            _currentProject = new ScadProject(_rootResolver);
            _currentProject.Load(savedProject, _currentFile);

            Open(_currentProject.MainModule);
            // again, important, must be done after the main module is opened
            _currentHistoryStack = new HistoryStack(_currentProject, GetEditorState());
            RefreshControls();

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

        public override void _Input(InputEvent evt)
        {
            if (evt.IsUndo() && _currentHistoryStack.CanUndo(out _))
            {
                Undo();
            }

            if (evt.IsRedo() && _currentHistoryStack.CanRedo(out _))
            {
                Redo();
            }
        }
    }


    class NodeBasedEntry : IAddDialogEntry
    {
        private readonly ScadNode _template;
        private readonly Func<ScadNode> _factory;
        private readonly Action<RequestContext, NodeBasedEntry> _action;

        public string Title => _template.NodeTitle;
        public string Keywords => _template.NodeDescription;
        public Action<RequestContext> Action => Select;

        private void Select(RequestContext obj)
        {
            _action(obj, this);
        }

        public Texture Icon { get; }

        public ScadNode CreateNode() => _factory();

        public NodeBasedEntry(Texture icon, Func<ScadNode> factory, Action<RequestContext, NodeBasedEntry> action)
        {
            _factory = factory;
            _template = _factory();
            _action = action;
            Icon = icon;
        }

        public EntryFittingDecision CanAdd(RequestContext context)
        {
            if (!context.Source.Description.CanUse(_template))
            {
                // if the node is not allowed to be used here, we can't use it
                return EntryFittingDecision.Veto;
            }

            // if this came from a node left of us, check if we have a matching input port
            if (context.SourceNode != null)
            {
                for (var i = 0; i < _template.InputPortCount; i++)
                {
                    var connection = new ScadConnection(context.Source, context.SourceNode, context.LastPort, _template,
                        i);
                    if (ConnectionRules.CanConnect(connection).Decision == ConnectionRules.OperationRuleDecision.Allow)
                    {
                        return EntryFittingDecision.Fits;
                    }
                }
            }

            // if this came from a node right of us, check if we have a matching output port
            if (context.DestinationNode != null)
            {
                for (var i = 0; i < _template.OutputPortCount; i++)
                {
                    var connection = new ScadConnection(context.Source, _template, i, context.DestinationNode,
                        context.LastPort);
                    if (ConnectionRules.CanConnect(connection).Decision == ConnectionRules.OperationRuleDecision.Allow)
                    {
                        return EntryFittingDecision.Fits;
                    }
                }
            }

            // otherwise it doesn't match, but could still be added.
            return EntryFittingDecision.DoesNotFit;
        }
    }
}