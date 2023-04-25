using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.History;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Library.IO;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;
using OpenScadGraphEditor.Widgets.AddDialog;
using OpenScadGraphEditor.Widgets.CommentEditingDialog;
using OpenScadGraphEditor.Widgets.DocumentationDialog;
using OpenScadGraphEditor.Widgets.HelpDialog;
using OpenScadGraphEditor.Widgets.IconButton;
using OpenScadGraphEditor.Widgets.ImportDialog;
using OpenScadGraphEditor.Widgets.InvokableRefactorDialog;
using OpenScadGraphEditor.Widgets.LogConsole;
using OpenScadGraphEditor.Widgets.NodeColorDialog;
using OpenScadGraphEditor.Widgets.ProjectTree;
using OpenScadGraphEditor.Widgets.SettingsDialog;
using OpenScadGraphEditor.Widgets.StylusDebugDialog;
using OpenScadGraphEditor.Widgets.UsageDialog;
using OpenScadGraphEditor.Widgets.VariableCustomizer;
using OpenScadGraphEditor.Widgets.VariableRefactorDialog;
using Serilog;

namespace OpenScadGraphEditor
{
	[UsedImplicitly]
	public class GraphEditor : Control, ICanPerformRefactorings, IEditorContext
	{
		private AddDialog _addDialog;
		private QuickActionsPopup _quickActionsPopup;
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
		private BuiltInLibrary _rootResolver;
		private InvokableRefactorDialog _invokableRefactorDialog;
		private VariableRefactorDialog _variableRefactorDialog;
		private NodeColorDialog _nodeColorDialog;
		private CommentEditingDialog _commentEditingDialog;
		private StylusDebugDialog _stylusDebugDialog;
		private SettingsDialog _settingsDialog;
		private HelpDialog _helpDialog;
		private UsageDialog _usageDialog;
		private DocumentationDialog _documentationDialog;
		private readonly List<IAddDialogEntry> _addDialogEntries = new List<IAddDialogEntry>();
		private Foldout _leftFoldout;
		private VariableCustomizer _variableCustomizer;
		private SplitContainer _editingInterface;

		private readonly List<IAddDialogEntryFactory> _addDialogEntryFactories =
			typeof(IAddDialogEntryFactory)
				.GetImplementors()
				.CreateInstances<IAddDialogEntryFactory>()
				.ToList();

		private readonly List<IEditorAction> _editorActions =
			typeof(IEditorActionFactory)
				.GetImplementors()
				.CreateInstances<IEditorActionFactory>()
				.SelectMany(it => it.CreateActions())
				.ToList();

		private ScadGraph _copyBuffer;
		private LogConsole _logConsole;
		private Foldout _lowerFoldout;

		private bool _codeChanged;
		private bool _refactoringInProgress;
		private readonly Dictionary<string, ScadGraphEdit> _openEditors = new Dictionary<string, ScadGraphEdit>();

		private readonly Configuration _configuration = new Configuration();
		private IconButton _openOpenScadButton;


		public override void _Ready()
		{
			_logConsole = this.WithName<LogConsole>("LogConsole");
			_lowerFoldout = this.WithName<Foldout>("LowerFoldout");
			var logFile = OS.GetUserDataDir() + "/log.txt";

			OS.SetWindowTitle($"OpenSCAD Graph Editor ({AppVersion.Version})");

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.WriteTo.File(logFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5)
				.WriteTo.Sink(_logConsole)
				.CreateLogger();
			GD.Print("OpenSCAD log file is: " + logFile);
			Log.Information("Initializing OpenScad Graph Editor");

			OS.LowProcessorUsageMode = true;

			_logConsole.OpenLogFileRequested += () => OS.ShellOpen(logFile);

			_configuration.Load();

			// scale all themes to editor scale
			var percent = _configuration.GetEditorScalePercent();
			foreach (var theme in Resources.AllThemes)
			{
				theme.Scale(percent);
			}

			_rootResolver = new BuiltInLibrary();

			_tabContainer = this.WithName<TabContainer>("TabContainer");
			_tabContainer
				.Connect("tab_changed")
				.To(this, nameof(OnTabChanged));

			_addDialog = this.WithName<AddDialog>("AddDialog");
			_quickActionsPopup = this.WithName<QuickActionsPopup>("QuickActionsPopup");
			_importDialog = this.WithName<ImportDialog>("ImportDialog");
			_importDialog.OnNewImportRequested += OnNewImportRequested;
			_importDialog.OnUpdateImportRequested += OnUpdateImportRequested;
			_usageDialog = this.WithName<UsageDialog>("UsageDialog");
			_usageDialog.NodeHighlightRequested += OnNodeHighlightRequested;

			_settingsDialog = this.WithName<SettingsDialog>("SettingsDialog");
			_settingsDialog.RefactoringRequested += PerformRefactoring;
			_stylusDebugDialog = this.WithName<StylusDebugDialog>("StylusDebugDialog");

			_helpDialog = this.WithName<HelpDialog>("HelpDialog");

			_commentEditingDialog = this.WithName<CommentEditingDialog>("CommentEditingDialog");
			_commentEditingDialog.RefactoringRequested += PerformRefactoring;

			_nodeColorDialog = this.WithName<NodeColorDialog>("NodeColorDialog");
			_nodeColorDialog.ColorSelected +=
				(graph, node, color) => PerformRefactorings("Change node color",
					new ToggleModifierRefactoring(graph, node, ScadNodeModifier.Color, true, color));

			_nodeColorDialog.ColorCleared +=
				(graph, node) => PerformRefactorings("Clear node color",
					new ToggleModifierRefactoring(graph, node, ScadNodeModifier.Color, false));


			_invokableRefactorDialog = this.WithName<InvokableRefactorDialog>("InvokableRefactorDialog");
			_invokableRefactorDialog.InvokableModificationRequested +=
				(description, refactorings) => PerformRefactorings($"Change {description.Name}", refactorings);
			_invokableRefactorDialog.InvokableCreationRequested +=
				// create the invokable, then open it's graph in a new tab.
				(description, refactorings) => PerformRefactorings($"Create {description.Name}", refactorings,
					() => Open(_currentProject.FindDefiningGraph(description)));

			_documentationDialog = this.WithName<DocumentationDialog>("DocumentationDialog");
			_documentationDialog.RefactoringRequested +=
				(refactoring) => PerformRefactorings("Edit documentation", refactoring);

			_variableRefactorDialog = this.WithName<VariableRefactorDialog>("VariableRefactorDialog");
			_variableRefactorDialog.RefactoringsRequested +=
				(refactorings) => PerformRefactorings("Change variable settings", refactorings);

			_editingInterface = this.WithName<SplitContainer>("EditingInterface");
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

			this.WithName<Button>("SettingsButton")
				.Connect("pressed")
				.To(this, nameof(OnSettingsButtonPressed));

			_openOpenScadButton = this.WithName<IconButton>("OpenOpenScadButton");
			_openOpenScadButton
				.ButtonPressed += OnOpenOpenScadButtonPressed;

			var openButton = this.WithName<OpenButton>("OpenButton");
			openButton.OpenFileRequested += OnOpenFile;
			openButton.OpenFileDialogRequested += OnOpenFileDialogRequested;
			openButton.Init(_configuration);

			_leftFoldout = this.WithName<Foldout>("LeftFoldout");
			_leftFoldout.OnFoldoutChanged += (visible) => { _editingInterface.Collapsed = !visible; };

			_variableCustomizer = this.WithName<VariableCustomizer>("VariableCustomizer");
			_variableCustomizer.RefactoringRequested += PerformRefactoring;
			_variableCustomizer.VariableEditingRequested +=
				(variable) => _variableRefactorDialog.Open(variable, _currentProject);

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

			_copyBuffer = new ScadGraph();
			OnNewButtonPressed();
			NotificationService.ShowNotification("Welcome to OpenScad Graph Editor");
		}

		private void OnOpenOpenScadButtonPressed()
		{
			var path = _configuration.GetOpenScadPath();
			if (path.Empty())
			{
				NotificationService.ShowNotification("OpenSCAD path not set. Please set it in the settings dialog.");
				return;
			}

			NotificationService.ShowNotification("Opening OpenSCAD. Please wait a few seconds for it to open.");
			// ReSharper disable once StringLiteralTypo
			OS.Execute(path, new[] {"--viewall", _currentFile + ".scad"}, blocking: false);
		}


		private void OnSettingsButtonPressed()
		{
			_settingsDialog.Open(_configuration, _currentProject);
		}


		private void OnTabChanged(int index)
		{
			var editor = _tabContainer.GetChild<ScadGraphEdit>(index);
			if (editor.Graph != null)
			{
				// re-paint
				editor.Render(_currentProject, editor.Graph);
			}
		}


		private void OnNodeHighlightRequested(UsagePointInformation information)
		{
			var graph = _currentProject.AllDeclaredInvokables.FirstOrDefault(it =>
				it.Description.Id == information.GraphId);
			var node = graph?.GetAllNodes().FirstOrDefault(it => it.Id == information.NodeId);
			if (node != null)
			{
				var edit = Open(graph);
				edit.FocusNode(node);
				return;
			}

			NotificationService.ShowNotification("This usage no longer exists");
		}

		private void OnNewImportRequested(string path, IncludeMode includeMode)
		{
			PerformRefactoring($"Add reference to {path}",
				new AddOrUpdateExternalReferenceRefactoring(path, includeMode));
		}

		private void OnUpdateImportRequested(ExternalReference reference, string path, IncludeMode includeMode)
		{
			PerformRefactoring($"Update reference to {path}",
				new AddOrUpdateExternalReferenceRefactoring(path, includeMode));
		}

		private void OnItemContextMenuRequested(ProjectTreeEntry entry, Vector2 mousePosition)
		{
			var actions = new List<QuickAction>();
			var requestContext = entry switch
			{
				ScadInvokableTreeEntry invokableTreeEntry => RequestContext.ForInvokableDescription(invokableTreeEntry.Description),
				ExternalReferenceTreeEntry externalReferenceTreeEntry => RequestContext.ForExternalReference(externalReferenceTreeEntry.Description),
				ScadVariableTreeEntry variableTreeEntry => RequestContext.ForVariableDescription(variableTreeEntry.Description),
				_ => null
			};

			if (requestContext == null)
			{
				return;
			}
			
			foreach (var editorAction in _editorActions)
			{
				if (editorAction.TryBuildQuickAction(this, requestContext, out var quickAction))
				{
					actions.Add(quickAction);
				}
			}

			_quickActionsPopup.Open(mousePosition, actions);
		}

		/// <summary>
		/// Called when an entry is double clicked in the project tree.
		/// </summary>
		private void Open(ProjectTreeEntry entry)
		{
			if (entry is ScadInvokableTreeEntry invokableTreeEntry
				&& _currentProject.IsDefinedInThisProject(invokableTreeEntry.Description))
			{
				OpenGraph(invokableTreeEntry.Description);
			}

			if (entry is ScadVariableTreeEntry variableTreeEntry
				&& _currentProject.IsDefinedInThisProject(variableTreeEntry.Description))
			{
				OpenRefactorDialog(variableTreeEntry.Description);
			}

			if (entry is ExternalReferenceTreeEntry externalReferenceTreeEntry &&
				// we dont want to open the import dialog for transitive imports
				!externalReferenceTreeEntry.Description.IsTransitive)
			{
				OpenRefactorDialog(externalReferenceTreeEntry.Description);
			}
		}

		private void Clear()
		{
			_currentProject = null;
			_currentHistoryStack = null;
			_currentFile = null;
			_fileNameLabel.Text = "<not yet saved to a file>";
			_tabContainer.GetChildNodes<ScadGraphEdit>().ForAll(it => it.RemoveAndFree());
			_openEditors.Clear();
			_projectTree.Clear();
			_openOpenScadButton.Disabled = true;
			_openOpenScadButton.HintTooltip = "(disabled) save project to enable";
		}


		private void RefreshControls()
		{
			_undoButton.Disabled = !_currentHistoryStack.CanUndo(out var undoDescription);
			_undoButton.HintTooltip = $"Undo : {undoDescription}";

			_redoButton.Disabled = !_currentHistoryStack.CanRedo(out var redoDescription);
			_redoButton.HintTooltip = $"Redo : {redoDescription}";

			_projectTree.Setup(new List<ProjectTreeEntry>() {new RootProjectTreeEntry(_currentProject)});

			_variableCustomizer.Setup(_currentProject.Variables.ToList());

			// Re-Build the list of entries for the add dialog.

			_addDialogEntries.Clear();
			_addDialogEntries.AddRange(
				_addDialogEntryFactories.SelectMany(it => it.BuildEntries(_currentProject, this))
			);
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


		private void AddNode(RequestContext context, ScadNode node)
		{
			context.TryGetNodeAndPort(out var graph, out var otherNode, out var otherPort, out var position);
			node.Offset = position;

			PerformRefactorings("Add node",
				new AddNodeRefactoring(graph, node, otherNode, otherPort));
		}

		private ScadGraphEdit Open(ScadGraph toOpen)
		{
			// check if it is already open
			if (_openEditors.TryGetValue(toOpen.Description.Id, out var existingEditor))
			{
				_tabContainer.CurrentTab = existingEditor.GetIndex();
				existingEditor.Render(_currentProject, toOpen);
				return existingEditor;
			}

			// if not, open a new tab
			var editor = Prefabs.New<ScadGraphEdit>();
			editor.RefactoringsRequested += PerformRefactorings;
			editor.NodePopupRequested += OnNodeContextMenuRequested;
			editor.ItemDataDropped += OnItemDataDropped;
			editor.AddDialogRequested += OnAddDialogRequested;
			editor.CopyRequested += OnCopyRequested;
			editor.DuplicateRequested += OnDuplicateRequested;
			editor.PasteRequested += OnPasteRequested;
			editor.CutRequested += OnCutRequested;
			editor.EditCommentRequested += OnEditCommentRequested;
			editor.HelpRequested += OnHelpRequested;

			editor.Name = toOpen.Description.Id;
			editor.MoveToNewParent(_tabContainer);
			editor.Render(_currentProject, toOpen);
			_openEditors[toOpen.Description.Id] = editor;
			_tabContainer.CurrentTab = _tabContainer.GetChildCount() - 1;
			_tabContainer.SetTabTitle(_tabContainer.CurrentTab, editor.Graph.Description.NodeNameOrFallback);
			editor.FocusEntryPoint();
			return editor;
		}

		private void OnHelpRequested(RequestContext obj)
		{
			if (obj.TryGetNode(out var graph, out var node, out _))
			{
				_helpDialog.Open(_currentProject, graph, node);
			}
		}

		private void Close(ScadGraphEdit editor)
		{
			_openEditors.Remove(editor.Graph.Description.Id);
			editor.RemoveAndFree();
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
				.Select(it => new DeleteNodeRefactoring(source.Graph, it))
				.ToList();

			PerformRefactorings("Cut nodes", deleteRefactorings);
		}

		/// <summary>
		///  Called when the user wants to edit the comment of a node.
		/// </summary>
		private void OnEditCommentRequested(RequestContext requestContext)
		{
			if (!requestContext.TryGetNode(out var graph, out var node, out _))
			{
				return;
			}
			EditComment(graph, node);
		}

		/// <summary>
		/// Copies the given nodes from the source graph into a copy buffer.
		/// </summary>
		private ScadGraph MakeCopyBuffer(ScadGraph source, IEnumerable<ScadNode> selection)
		{
			var result = new ScadGraph();
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
				// and bound nodes later.
				idMapping[node.Id] = copy.Id;
				result.AddNode(copy);
			}

			// correct the id mappings of any bound nodes
			foreach (var node in result.GetAllNodes().Where(it => it is IAmBoundToOtherNode))
			{
				var partnerId = ((IAmBoundToOtherNode) node).OtherNodeId;
				// find out which id the partner has in the copy
				var partnerCopyId = idMapping[partnerId];
				// and set the partner id to the copy id
				((IAmBoundToOtherNode) node).OtherNodeId = partnerCopyId;
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
			_copyBuffer = SelectionToBuffer(source, selection);
		}

		private void OnDuplicateRequested(ScadGraphEdit source, List<ScadNode> selection, Vector2 position)
		{
			var copies = SelectionToBuffer(source, selection);
			PasteNodes(source, position, copies, "Duplicate nodes");
		}

		private ScadGraph SelectionToBuffer(ScadGraphEdit source, List<ScadNode> selection)
		{
			// when we make a copy we need to take special care about bound nodes
			// each bound node needs to have its partner within the copy even if it was not originally selected.
			// so we look up all bound nodes in the selection and if its partner is not in the
			// selection we silently add it.

			var correctedSelection = new List<ScadNode>(selection);

			var boundNodes = selection.Where(it => it is IAmBoundToOtherNode).ToList();
			foreach (var boundNode in boundNodes)
			{
				var boundTo = (IAmBoundToOtherNode) boundNode;
				var partner = source.Graph.ById(boundTo.OtherNodeId);

				if (!selection.Contains(partner))
				{
					correctedSelection.Add(partner);
				}
			}

			// now we can be sure that we have no bound nodes without their partner in the corrected selection.
			return MakeCopyBuffer(source.Graph, correctedSelection);
		}

		private void OnPasteRequested(ScadGraphEdit target, Vector2 position)
		{
			// now make another copy from the copy buffer and paste that. The reason we do this is because
			// nodes need unique Ids so for each pasting we need to make a new copy.
			var copy = MakeCopyBuffer(_copyBuffer, _copyBuffer.GetAllNodes());

			PasteNodes(target, position, copy, "Paste nodes");
		}

		private void PasteNodes(ScadGraphEdit target, Vector2 position, ScadGraph copy, string refactoringTitle)
		{
			// now the clipboard may contain nodes that are not allowed in the given target graph. So we need
			// to filter these out here and also delete all connections to these nodes.
			var disallowedNodes = copy.GetAllNodes()
				.Where(it => !target.Graph.Description.CanUse(it))
				.ToList();

			// delete all connections to the disallowed nodes
			copy.GetAllConnections()
				.Where(it => disallowedNodes.Any(it.InvolvesNode))
				.ToList() // avoid concurrent modification
				.ForAll(copy.RemoveConnection);

			// and the nodes themselves
			disallowedNodes.ForAll(copy.RemoveNode);

			var scadNodes = copy.GetAllNodes().ToList();
			if (scadNodes.Count == 0)
			{
				return;
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
				refactorings.Add(new AddNodeRefactoring(target.Graph, node));
			}

			foreach (var connection in copy.GetAllConnections())
			{
				refactorings.Add(
					new AddConnectionRefactoring(
						new ScadConnection(target.Graph, connection.From, connection.FromPort, connection.To,
							connection.ToPort
						)
					)
				);
			}

			PerformRefactorings(refactoringTitle, refactorings, () => target.SelectNodes(scadNodes));
		}

		private void OnAddFunctionPressed()
		{
			_invokableRefactorDialog.OpenForNewFunction(_currentProject);
		}

		private void OnAddVariablePressed()
		{
			_variableRefactorDialog.OpenForNewVariable(_currentProject);
		}

		private void OnAddModulePressed()
		{
			_invokableRefactorDialog.OpenForNewModule(_currentProject);
		}

		public void PerformRefactorings(string description, params Refactoring[] refactorings)
		{
			PerformRefactorings(description, (IEnumerable<Refactoring>) refactorings);
		}

		public void PerformRefactorings(string description, IEnumerable<Refactoring> refactorings,
			params Action[] after)
		{
			try
			{
				GdAssert.That(!_refactoringInProgress,
					"Cannot run a refactoring while a refactoring is running. Probably some UI throws events when internal state is updated.");

				_refactoringInProgress = true;
				Log.Debug(">> Refactorings start");

				var top = _currentHistoryStack.Peek();
				var currentEditorState = GetEditorState();
				// the editor state could be different from how it was when we finished the previous refactoring (e.g tab changed or 
				// the editor was moved). we therefore patch the new editor state onto the top of the stack.
				// so we can return to that state once we undo this refactoring. This way we fix a problem that
				// when you open a new tab, and move a node there, then undo that the new tab is closed.
				// it is important to patch this _before_ we do the refactoring otherwise we may refer to
				// data that is no longer there after the refactoring.
				top.PatchEditorState(currentEditorState);

				var context = new RefactoringContext(_currentProject);
				context.PerformRefactorings(refactorings);

				// close all tabs referring to graphs which are no longer in the project
				_tabContainer.GetChildNodes<ScadGraphEdit>()
					.Where(it => !_currentProject.IsDefinedInThisProject(it.Graph.Description))
					.ToList()
					.ForAll(Close);

				if (_tabContainer.CurrentTab >= _tabContainer.GetTabCount())
				{
					// open the left most tab
					_tabContainer.CurrentTab = 0;
				}

				for (var i = 0; i < _tabContainer.GetTabCount(); i++)
				{
					var graphEdit = (ScadGraphEdit) _tabContainer.GetTabControl(i);
					// update the graph renderings.
					graphEdit.Render(_currentProject, graphEdit.Graph);
					// and update the tab title as it might have changed.
					_tabContainer.SetTabTitle(i, graphEdit.Graph.Description.NodeNameOrFallback);
				}

				// important, the snapshot must be made _after_ the changes.
				_currentHistoryStack.AddSnapshot(description, _currentProject, GetEditorState());


				RefreshControls();
				MarkDirty(true);
				_refactoringInProgress = false;
				Log.Debug("<< Refactorings end");

				after.ForAll(it => it());
			}
			catch (Exception e)
			{
				NotificationService.ShowError(
					"Unexpected exception occurred. Running undo to restore a known working state. See console for details.");
				Log.Error(e, "Unexpected exception");
				Undo();
			}
		}

		private void OnNewButtonPressed()
		{
			Clear();
			_currentProject = new ScadProject(_rootResolver)
			{
				Preamble = _configuration.GetDefaultPreamble()
			};
			Open(_currentProject.MainModule);
			// important, this needs to be done after the main module is opened, so we get correct editor state
			_currentHistoryStack = new HistoryStack(_currentProject, GetEditorState());
			RefreshControls();
			RenderScadOutput();
		}

		private EditorState GetEditorState()
		{
			// create a list of currently open tabs

			// order is important here, so we get the child order from the _tabContainer rather than using
			// _openEditors
			var tabs = _tabContainer.GetChildNodes<ScadGraphEdit>()
				.Select(it => new EditorOpenTab(it.Graph.Description.Id, it.ScrollOffset)).ToList();
			return new EditorState(tabs, _tabContainer.CurrentTab);
		}

		private void RestoreEditorState(EditorState editorState)
		{
			// close all open tabs
			foreach (var tab in _openEditors.Values.ToList())
			{
				Close(tab);
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
				var rerouteNode = NodeFactory.Build<RerouteNode>();
				// when also CTRL is down, we make the reroute node a wireless node
				if (Input.IsKeyPressed((int) KeyList.Control))
				{
					rerouteNode.IsWireless = true;
				}

				AddNode(context, rerouteNode);
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

				// ensure we don't drag in stuff that isn't usable by the graph in question.
				if (!graph.Graph.Description.CanUse(node))
				{
					return;
				}

				node.Offset = virtualPosition;
				PerformRefactoring("Add node", new AddNodeRefactoring(graph.Graph, node));
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
						() => PerformRefactoring("Add node", new AddNodeRefactoring(graph.Graph, getNode))),
					new QuickAction($"Set {variableListEntry.Description.Name}",
						() => PerformRefactoring("Add node", new AddNodeRefactoring(graph.Graph, setNode)))
				};

				_quickActionsPopup.Open(mousePosition, actions);
			}
		}

		private void OnNodeContextMenuRequested(RequestContext requestContext)
		{
			requestContext.TryGetNode(out var graph, out var node, out var position);

			// build a list of quick actions that include all refactorings that would apply to the selected node
			var actions = UserSelectableNodeRefactoring
				.GetApplicable(graph, node)
				.OrderBy(it => it.Order)
				.GroupBy(it => it.Group)
				.Select(it =>
				{
					if (it.Count() > 1)
					{
						// group into submenu
						return new QuickAction(it.Key, it.Select(refactoring =>
							new QuickAction(refactoring.Title,
								() => PerformRefactoring(refactoring.Title, refactoring))).ToList());
					}

					// just return the item
					var refactoring = it.First();
					return new QuickAction(refactoring.Title,
						() => PerformRefactoring(refactoring.Title, refactoring));
				});

		
		
			

			_quickActionsPopup.Open(position, actions);
		}

		private void OnOpenFileDialogRequested()
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
				NotificationService.ShowError($"File {filename} does not exist anymore.");
				return;
			}

			// set the directory of the file the current directory in the file open dialog
			_fileDialog.CurrentDir = PathResolver.GetDirectoryFromFile(filename);

			var savedProject = ResourceLoader.Load<SavedProject>(filename, "", noCache: true);
			if (savedProject == null)
			{
				NotificationService.ShowError("Cannot load file " + filename);
				return;
			}

			// make backup
			FileBackups.BackupFile(filename, _configuration.GetNumberOfBackups());


			// workaround for https://github.com/godotengine/godot/issues/59686
			// as the "noCache" flag currently isn't working properly
			savedProject.ResourcePath = "";

			Clear();

			_currentFile = filename;
			_configuration.AddRecentFile(filename);
			_fileNameLabel.Text = filename;
			_openOpenScadButton.Disabled = false;
			_openOpenScadButton.HintTooltip = "";

			_currentProject = new ScadProject(_rootResolver);
			try
			{
				_currentProject.Load(savedProject, _currentFile);
			}
			catch (Exception e)
			{
				NotificationService.ShowError("Error when loading file. See log for details.");
				Log.Error(e, "Exception when loading file {File}", _currentFile);
				// don't load a half-broken file
				OnNewButtonPressed();
				return;
			}

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
			_configuration.AddRecentFile(filename);
			_fileNameLabel.Text = filename;
			_openOpenScadButton.Disabled = false;
			_openOpenScadButton.HintTooltip = "";
			// repaint all open graphs as the file name is now known and this enables relative paths
			// in node path references
			for (var i = 0; i < _tabContainer.GetTabCount(); i++)
			{
				var graphEdit = (ScadGraphEdit) _tabContainer.GetTabControl(i);
				// update the graph renderings.
				graphEdit.Render(_currentProject, graphEdit.Graph);
			}
			
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
			_forceSaveButton.Text = _codeChanged ? "[.!.]" : "[...]";
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
				var error = ResourceSaver.Save(_currentFile, savedProject);
				if (error != Error.Ok)
				{
					NotificationService.ShowNotification("Cannot save project, see log for details");
					Log.Warning("Unable to save project to {File} -> Error {Error}", _currentFile, error);
				}
				else
				{
					Log.Information("Saved project");
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
				var outputPath = _currentFile + ".scad";
				var error = file.Open(outputPath, File.ModeFlags.Write);
				if (error == Error.Ok)
				{
					file.StoreString(rendered);
					file.Close();
				}
				else
				{
					NotificationService.ShowError("Cannot save SCAD file, see log for details");
					Log.Warning("Unable to save SCAD output file to {File} -> Error {Error}", outputPath, error);
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

			if (evt.IsStylusDebug())
			{
				_stylusDebugDialog.Show();
			}
		}

		public void FindAndShowUsages(VariableDescription variableDescription)
		{
			var usagePoints = _currentProject.FindAllReferencingNodes(variableDescription)
				.Select(it => new UsagePointInformation($"{it.Node.NodeTitle} in ({it.Graph.Description.Name})",
					it.Graph.Description.Id, it.Node.Id))
				.ToList();

			if (usagePoints.Count == 0)
			{
				NotificationService.ShowNotification("No usages found.");
				return;
			}

			_usageDialog.Open($"Usages of variable {variableDescription.Name}", usagePoints);
			_lowerFoldout.ShowChild(_usageDialog);
		}

		public void FindAndShowUsages(InvokableDescription invokableDescription)
		{
			var usagePoints = _currentProject.FindAllReferencingNodes(invokableDescription)
				.Select(it => new UsagePointInformation($"{it.Node.NodeTitle} in ({it.Graph.Description.Name})",
					it.Graph.Description.Id, it.Node.Id))
				.ToList();

			if (usagePoints.Count == 0)
			{
				NotificationService.ShowNotification("No usages found.");
				return;
			}

			var type = invokableDescription is FunctionDescription ? "function" : "module";
			_usageDialog.Open($"Usages of {type} {invokableDescription.Name}", usagePoints);
			_lowerFoldout.ShowChild(_usageDialog);
		}

		public override void _Notification(int what)
		{
			if (what == NotificationWmAbout)
			{
				// OSX about menu.
				NotificationService.ShowNotification($"This is OpenSCAD Graph Editor ({AppVersion.Version}");
			}
		}

		public ScadProject CurrentProject => _currentProject;
		
		public void OpenRefactorDialog(InvokableDescription invokableDescription)
		{
			_invokableRefactorDialog.Open(invokableDescription, _currentProject);
		}

		public void OpenRefactorDialog(VariableDescription variableDescription)
		{
			_variableRefactorDialog.Open(variableDescription, _currentProject);
		}

		public void OpenRefactorDialog(ExternalReference externalReference)
		{
			_importDialog.OpenForExistingImport(externalReference, _currentProject.ProjectPath);
		}

		public void OpenDocumentationDialog(InvokableDescription invokableDescription)
		{
			_documentationDialog.Open(invokableDescription);
		}

		public void PerformRefactoring(string description, Refactoring refactoring)
		{
			PerformRefactorings(description, refactoring);
		}

		public void OpenGraph(InvokableDescription invokableDescription)
		{
			Open(_currentProject.FindDefiningGraph(invokableDescription));
		}
		
		public void EditComment(ScadGraph graph, ScadNode node) {
			_commentEditingDialog.Open(graph, node);
		}

		public void EditNodeColor(ScadGraph graph, ScadNode node)
		{
			_nodeColorDialog.Open(graph, node);
		}
 	}
}
