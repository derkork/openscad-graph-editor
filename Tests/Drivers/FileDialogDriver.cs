using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotExt;
using GodotTestDriver.Drivers;
using GodotTestDriver.Util;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Tests.Drivers
{
    /// <summary>
    /// Driver for the <see cref="FileDialog"/> widget.
    /// </summary>
    [PublicAPI]
    public class FileDialogDriver<T> : ControlDriver<T> where T:FileDialog
    {
        public ButtonDriver OpenButton { get; }
        public ButtonDriver CancelButton { get; }
        public ButtonDriver SaveButton { get; }
        

        public FileDialogDriver(Func<T> producer, string description = "") : base(producer, description)
        {
            OpenButton = new ButtonDriver(
                () => Root?.FindAllDescendants<Button>(it => it.Text == TranslationServer.Translate("Open")).FirstOrDefault());
            CancelButton = new ButtonDriver(
                () => Root?.FindAllDescendants<Button>(it => it.Text == TranslationServer.Translate("Cancel")).FirstOrDefault());
            SaveButton = new ButtonDriver(
                () => Root?.FindAllDescendants<Button>(it => it.Text == TranslationServer.Translate("Save")).FirstOrDefault());
         
        }

        /// <summary>
        /// Changes the directory of the file dialog to the given directory.
        /// </summary>
        public async Task ChangeDirectory(string directory)
        {
            var root = VisibleRoot;
            root.CurrentDir = directory;
            await root.WaitForEvents();
            if (root.CurrentDir != directory)
            {
                throw new Exception($"Directory '{directory}' does not exist in: {Description}");
            }
        }

        /// <summary>
        /// Selects the given file in the currently open directory. The given <see cref="filename"/> must be a simple file name
        /// no path.
        /// </summary>
        public async Task SelectFile(string filename)
        {
            var root = VisibleRoot;
            root.CurrentFile = filename;
            await root.WaitForEvents();
            if (root.CurrentFile != filename)
            {
                throw new InvalidOperationException($"File '{filename}' does not exist in: {Description}");
            }
        }
        
    }
    
    /// <summary>
    /// Driver for the <see cref="FileDialog"/> widget.
    /// </summary>
    [PublicAPI]
    public class FileDialogDriver : FileDialogDriver<FileDialog>
    {
        public FileDialogDriver(Func<FileDialog> producer, string description = "") : base(producer, description)
        {
        }
    }
}