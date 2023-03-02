using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class FileSelector : LiteralWidgetBase<Control, StringLiteral>
    {

        private FileSelectBox.FileSelectBox _fileSelectBox;
        private CheckBox _relativeCheckBox;
        private string _sourceFileForRelativePath = "";

        public string SourceFileForRelativePaths
        {
            get => _sourceFileForRelativePath;
            set
            {
                _sourceFileForRelativePath = value;
                var hasNoBaseDir = string.IsNullOrEmpty(_sourceFileForRelativePath);
                if ( _relativeCheckBox != null)
                {
                    _relativeCheckBox.Disabled = hasNoBaseDir;
                    if (hasNoBaseDir)
                    {
                        _relativeCheckBox.SetPressedNoSignal(false);
                    }
                }
            }
        }

        protected override Control CreateControl()
        {
            var result = new VBoxContainer();

            _fileSelectBox = Prefabs.InstantiateFromScene<FileSelectBox.FileSelectBox>();
            _fileSelectBox.OnFileSelected += OnFileSelected;
            _fileSelectBox.OnSelectPressed += () =>
            {
                _fileSelectBox.OpenSelectionDialog(PathResolver.GetDirectoryFromFile(SourceFileForRelativePaths));
            };

            result.AddChild(_fileSelectBox);
            _relativeCheckBox = new CheckBox();
            _relativeCheckBox.Text = "Relative";
            var hasNoBaseDir = string.IsNullOrEmpty(_sourceFileForRelativePath);
            _relativeCheckBox.Disabled = hasNoBaseDir;
            _relativeCheckBox.Pressed = !hasNoBaseDir;
            
            _relativeCheckBox.Connect("toggled", this, nameof(ToggleRelative));

            result.AddChild(_relativeCheckBox);
            return result;
        }

        protected override void ApplyControlValue()
        {
           _fileSelectBox.CurrentPath = Literal.Value;
           _relativeCheckBox.SetPressedNoSignal(PathResolver.IsRelativePath(Literal.Value));
           GD.Print("Relative: ", PathResolver.IsRelativePath(Literal.Value), " ", Literal.Value, " ", SourceFileForRelativePaths, "");
        }

        private void ToggleRelative(bool relative)
        {
            var result = CalculatePath(_fileSelectBox.CurrentPath);
            EmitValueChange(new StringLiteral(result));
        }
        
        private void OnFileSelected(string fileName)
        {
            var result = CalculatePath(fileName);

            EmitValueChange(new StringLiteral(result));
        }

        private string CalculatePath(string fileName)
        {
            var result = fileName;
            if (_relativeCheckBox.Pressed && !PathResolver.IsRelativePath(fileName))
            {
                // convert to relative path
                if (!PathResolver.TryAbsoluteToRelative(fileName, PathResolver.GetDirectoryFromFile( SourceFileForRelativePaths), out result))
                {
                    NotificationService.ShowError(
                        $"Cannot convert {fileName} to a relative path using {SourceFileForRelativePaths} as base dir. Will use absolute path instead.");
                    result = fileName;
                    _relativeCheckBox.Pressed = false;
                }
            }
            else if (!_relativeCheckBox.Pressed && PathResolver.IsRelativePath(fileName))
            {
                // convert to absolute path
                if (!PathResolver.TryResolve(fileName, SourceFileForRelativePaths, out result))
                {
                    NotificationService.ShowError(
                        $"Cannot convert {fileName} to an absolute path using {SourceFileForRelativePaths} as base dir. Will use relative path instead.");
                    result = fileName;
                    _relativeCheckBox.Pressed = true;
                }
            }

            return result;
        }
    }
}