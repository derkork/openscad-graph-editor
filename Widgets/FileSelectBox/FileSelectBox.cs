using System;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library.External;
using Path = System.IO.Path;

namespace OpenScadGraphEditor.Widgets.FileSelectBox
{
    /// <summary>
    /// A line edit with a button which allows to select a file.
    /// </summary>
    [UsedImplicitly]
    public class FileSelectBox : HBoxContainer
    {
        /// <summary>
        /// Event being raised when the select button is pressed.
        /// </summary>
        public event Action OnSelectPressed;

        /// <summary>
        /// Event being raised when a file is selected.
        /// </summary>
        public event Action<string> OnFileSelected;
        
        private FileDialog _fileDialog;
        private LineEdit _pathLineEdit;
        
        
        private string _currentPath = "";
        
        /// <summary>
        /// The path that is currently selected.
        /// </summary>
        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                _currentPath = value;
                if (_pathLineEdit != null)
                {
                    _pathLineEdit.Text = value;
                    _pathLineEdit.HintTooltip = value;
                }
            } 
        }


        /// <summary>
        /// The filters to set.
        /// </summary>
        public string[] Filters
        {
            get => _fileDialog.Filters;
            set => _fileDialog.Filters = value;
        }

        public override void _Ready()
        {
            _pathLineEdit = this.WithName<LineEdit>("PathLineEdit");
            _pathLineEdit.Text = _currentPath;
            _pathLineEdit.HintTooltip = _currentPath;
            
            _fileDialog = this.WithName<FileDialog>("_FileDialog");
            _fileDialog
                .Connect("file_selected")
                .To(this, nameof(RaiseFileSelected));
            
            this.WithName<Button>("SelectButton")
                .Connect("pressed")
                .To(this, nameof(RaiseSelectPressed));
            
        }

        private void RaiseFileSelected(string file)
        {
            _pathLineEdit.Text = file;
            OnFileSelected?.Invoke(file);
        }
        
        private void RaiseSelectPressed()
        {
            OnSelectPressed?.Invoke();
        }
        
        public void OpenSelectionDialog(string presetDirectory = "")
        {
            if (string.IsNullOrEmpty(presetDirectory) && !string.IsNullOrEmpty(CurrentPath))
            {
                    presetDirectory = PathResolver.GetDirectoryFromFile(CurrentPath);
            }
            
            if (!string.IsNullOrEmpty(presetDirectory))
            {
                _fileDialog.CurrentDir = presetDirectory;
            }

            _fileDialog.PopupCentered();
        }
    }
}