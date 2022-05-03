using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.IO;

namespace OpenScadGraphEditor.History
{
    /// <summary>
    /// A history stack item. It contains the name of the operation that was performed when this history
    /// item was created and the state of the project and the editor after the operation was performed.
    /// </summary>
    public class HistoryStackItem
    {
        public SavedProject ProjectState { get; }
        public EditorState EditorState { get; }
        public string OperationName { get; }
        
        public HistoryStackItem(string operationName, SavedProject projectState, EditorState editorState)
        {
            ProjectState = projectState;
            EditorState = editorState;
            OperationName = operationName;
        }
    }
}