using System;
using System.Collections.Generic;
using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Widgets
{
    public class OpenButton : MenuButton
    {
        public event Action<string> OpenFileRequested;
        public event Action OpenFileDialogRequested;
        private Configuration _configuration;
        
        private readonly List<Action> _currentActions = new List<Action>();
        
        
        public void Init(Configuration configuration)
        {
            _configuration = configuration;
        }

        public override void _Ready()
        {
            this.Connect("about_to_show")
                .To(this, nameof(OnAboutToShow));
            GetPopup()
                .Connect("id_pressed")
                .To(this, nameof(OnItemSelected));

        }


        private void OnAboutToShow()
        {

            var popup = GetPopup();
            popup.Clear();
            _currentActions.Clear();

            _currentActions.Add(RequestFileDialog);
            popup.AddItem("Open File...", _currentActions.Count-1);
            
            var recentItems = _configuration.GetRecentFiles();
            if (recentItems.Count > 0)
            {
                popup.AddSeparator("Recent files");
                // now add one line for each recent file and also add an action to open it
                foreach (var recentItem in recentItems)
                {
                    _currentActions.Add(() => OpenFileRequested?.Invoke(recentItem));
                    popup.AddItem(recentItem, _currentActions.Count - 1);
                }
            }
        }

        private void OnItemSelected(int id)
        {
            _currentActions[id]();            
        }
        
        private void RequestFileDialog()
        {
            OpenFileDialogRequested?.Invoke();
        }
        
        private void RequestFileOpen(string filePath)
        {
            OpenFileRequested?.Invoke(filePath);
        }
    }
}