using System;
using System.Collections.Generic;
using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.History
{
    /// <summary>
    /// This class is responsible for managing Undo/Redo history.
    /// </summary>
    public class HistoryStack
    {
        private readonly List<HistoryStackItem> _history = new List<HistoryStackItem>();
        private int _currentIndex;


        public HistoryStack(ScadProject initialProjectState, EditorState initialEditorState)
        {
            // make a history item for the initial state
            var initialHistoryItem = new HistoryStackItem("", initialProjectState.Save(), initialEditorState);
            
            _history.Add(initialHistoryItem);
            _currentIndex = 0;
        }
        
        /// <summary>
        /// Adds a snapshot to the history.
        /// </summary>
        public void AddSnapshot(string operationName, ScadProject project, EditorState editorState )
        {
            var savedProject = project.Save();
            // create a history stack item
            var item = new HistoryStackItem(operationName, savedProject, editorState);
            // add it to the history
            _history.Add(item);
            // set the current index to the new item
            _currentIndex++;
            // remove all history states after the current one
            if (_history.Count > _currentIndex + 1)
            {
                _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
            }
        }


        /// <summary>
        /// Drops the last history item and restores the state of the item that is now on top.
        /// </summary>
        public EditorState Undo(ScadProject project)
        {
            // we need at least 2 items in the history stack
            if (_currentIndex < 1)
            {
                throw new InvalidOperationException("Cannot undo. History stack is empty.");
            }
            
            var item = _history[_currentIndex - 1];
            // restore the project
            project.Load(item.ProjectState, project.ProjectPath);
            _currentIndex--;
            return item.EditorState;
        }

        /// <summary>
        /// Moves the top of the stack to the next item and restores the state of the item that is now on top.
        /// </summary>
        public EditorState Redo(ScadProject project)
        {
            // we need at least one item past the current index in the history stack
            if (_currentIndex >= _history.Count - 1)
            {
                throw new InvalidOperationException("Cannot redo. History has no more redo steps left.");
            }
            
            var item = _history[_currentIndex + 1];
            // restore the project
            project.Load(item.ProjectState, project.ProjectPath);
            _currentIndex++;
            return item.EditorState;
        }

        /// <summary>
        /// Checks if currently an undo step is available.
        /// </summary>
        public bool CanUndo(out string operationName)
        {
            // and undo step is available if we have at least two items in the history stack
            if (_currentIndex < 1)
            {
                operationName = "<no more undo steps>";
                return false;
            }
            operationName = _history[_currentIndex].OperationName;
            return true;
        }
        
        /// <summary>
        /// Checks if currently a redo step is available.
        /// </summary>
        public bool CanRedo(out string operationName)
        {
            // and redo step is available if we have at least one item past the current index in the history stack
            if (_currentIndex >= _history.Count - 1)
            {
                operationName = "<no more redo steps>";
                return false;
            }
            operationName = _history[_currentIndex + 1].OperationName;
            return true;
        }

    }
}