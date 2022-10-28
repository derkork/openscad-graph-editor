using System;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library.External;
using Path = System.IO.Path;

namespace OpenScadGraphEditor.Widgets.ImportDialog
{
    public class ImportDialog : WindowDialog
    {
        private OptionButton _importModeOptionButton;
        private OptionButton _pathModeOptionButton;
        private OptionButton _libraryFileOptionButton;
        private FileSelectBox.FileSelectBox _fileSelectBox;
        private Control _fileLabel;
        private Control _libraryFileLabel;
        private Control _libraryFileBox;

        [CanBeNull] private string _currentProjectPath;

        private string[] _allLibraryFiles;
        private Button _okButton;


        public event Action<string, IncludeMode> OnNewImportRequested;


        public override void _Ready()
        {
            _fileSelectBox = this.WithName<FileSelectBox.FileSelectBox>("FileSelectBox");
            _fileSelectBox.OnFileSelected += OnFileSelected;
            _fileSelectBox.OnSelectPressed += OnSelectButtonPressed;
            _fileSelectBox.Filters = new[] {"*.scad;OpenSCAD files"};
            
            _fileLabel = this.WithName<Control>("FileLabel");
            _libraryFileLabel = this.WithName<Control>("LibraryFileLabel");
            _libraryFileBox = this.WithName<Control>("LibraryFileBox");

            _libraryFileOptionButton = this.WithName<OptionButton>("LibraryFileOptionButton");
            _libraryFileOptionButton.Connect("item_selected")
                .To(this, nameof(OnLibraryFileChanged));
            _importModeOptionButton = this.WithName<OptionButton>("ImportModeOptionButton");
            _pathModeOptionButton = this.WithName<OptionButton>("PathModeOptionButton");

            _importModeOptionButton.AddItem("Use", (int) IncludeMode.Use);
            _importModeOptionButton.AddItem("Include", (int) IncludeMode.Include);

            _pathModeOptionButton.AddItem("Relative", (int) ExternalFilePathMode.Relative);
            _pathModeOptionButton.AddItem("Absolute", (int) ExternalFilePathMode.Absolute);
            _pathModeOptionButton.AddItem("Library", (int) ExternalFilePathMode.Library);

            _pathModeOptionButton
                .Connect("item_selected")
                .To(this, nameof(OnPathModeSelected));

            this.WithName<Button>("LibraryFileRefreshButton")
                .Connect("pressed")
                .To(this, nameof(RefreshLibraryFiles));

            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancelButtonPressed));

            _okButton = this.WithName<Button>("OkButton");
            _okButton
                .Connect("pressed")
                .To(this, nameof(OnOkButtonPressed));


            this.WithName<Button>("SelectButton")
                .Connect("pressed")
                .To(this, nameof(OnSelectButtonPressed));
        }


        public void OpenForNewImport([CanBeNull] string currentProjectPath)
        {
            _currentProjectPath = currentProjectPath;
            _importModeOptionButton.Selected = 0;
            _pathModeOptionButton.Selected = 0;

            // if the current project path is not yet known, do not allow relative paths
            if (string.IsNullOrEmpty(currentProjectPath))
            {
                _pathModeOptionButton.Selected = 1;
                _pathModeOptionButton.SetItemDisabled(0, true);
                _pathModeOptionButton.SetItemText(0, "Relative (disabled, save project first)");
            }
            else
            {
                _pathModeOptionButton.SetItemDisabled(0, false);
                _pathModeOptionButton.SetItemText(0, "Relative");
            }

            RefreshLibraryFiles();
            RefreshUi();
            PopupCentered();
        }


        private void OnLibraryFileChanged([UsedImplicitly] int _)
        {
            RefreshUi();
        }
        
        private void RefreshUi()
        {
            var pathMode = (ExternalFilePathMode) _pathModeOptionButton.GetSelectedId();
            _fileLabel.Visible = pathMode != ExternalFilePathMode.Library;
            _fileSelectBox.Visible = pathMode != ExternalFilePathMode.Library;

            _libraryFileLabel.Visible = pathMode == ExternalFilePathMode.Library;
            _libraryFileBox.Visible = pathMode == ExternalFilePathMode.Library;


            if (pathMode == ExternalFilePathMode.Library)
            {
                // need to select a library file 
                _okButton.Disabled = _libraryFileOptionButton.Selected == -1 || _allLibraryFiles.Length == 0;
            }
            else
            {
                // if we are file mode, we need to have a file selected
                var file = _fileSelectBox.CurrentPath;
                if (string.IsNullOrEmpty(file))
                {
                    _okButton.Disabled = true;
                }
                else
                {
                    // if we are in relative mode, try if we can resolve the file relative to the project
                    // file. This should always work on Linux or Mac but on Windows thanks to having drive letters
                    // it may not be possible. 

                    if (pathMode == ExternalFilePathMode.Relative)
                    {
                        var canResolve =
                            PathResolver.TryAbsoluteToRelative(_fileSelectBox.CurrentPath, Path.GetDirectoryName(_currentProjectPath), out _);
                        _okButton.Disabled = !canResolve;
                    }
                    else
                    {
                        _okButton.Disabled = false;
                    }
                }
            }
        }

        private void OnOkButtonPressed()
        {
                // new import
                var pathMode = (ExternalFilePathMode) _pathModeOptionButton.GetSelectedId();
                var includeMode = (IncludeMode) _importModeOptionButton.GetSelectedId();
                
                // build the external reference
                switch (pathMode)
                {
                    case ExternalFilePathMode.Library:
                        OnNewImportRequested?.Invoke(_allLibraryFiles[_libraryFileOptionButton.GetSelectedId()], includeMode);
                        break;
                    case ExternalFilePathMode.Relative:
                        var canResolve =
                            PathResolver.TryAbsoluteToRelative(_fileSelectBox.CurrentPath, Path.GetDirectoryName(_currentProjectPath), out var path);
                        GdAssert.That(canResolve, "Could not resolve path");
                        OnNewImportRequested?.Invoke(path, includeMode);
                        break;
                    case ExternalFilePathMode.Absolute:
                        OnNewImportRequested?.Invoke(_fileSelectBox.CurrentPath, includeMode);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Hide();
        }

        private void OnPathModeSelected([UsedImplicitly] int _)
        {
            RefreshUi();
        }

        private void OnSelectButtonPressed()
        {
            // if we already have a file selected, use the file's directory as the initial directory
            string presetDir;
            var file = _fileSelectBox.CurrentPath;
            if (!string.IsNullOrEmpty(file))
            {
                presetDir = Path.GetDirectoryName(file);
            }
            // if not, try to run with the current project's path
            else if (!string.IsNullOrEmpty(_currentProjectPath))
            {
                presetDir = _currentProjectPath;
            }
            else
            {
                // run with the user's documents folder
                presetDir = PathResolver.GetUsersDocumentsFolder();
            }

            _fileSelectBox.OpenSelectionDialog(presetDir);
        }


        private void OnFileSelected(string path)
        {
            RefreshUi();
        }

        private void RefreshLibraryFiles()
        {
            _libraryFileOptionButton.Clear();
            _allLibraryFiles = PathResolver.GetAllLibraryFiles();
            foreach (var libraryFile in _allLibraryFiles)
            {
                _libraryFileOptionButton.AddItem(libraryFile);
            }

            // add a dummy entry in case no library files exist
            if (_allLibraryFiles.Length == 0)
            {
                _libraryFileOptionButton.AddItem("<No library files found>");
                _libraryFileOptionButton.SetItemDisabled(0, true);
            }

            _libraryFileOptionButton.Selected = -1;
        }

        private void OnCancelButtonPressed()
        {
            Hide();
        }
    }
}