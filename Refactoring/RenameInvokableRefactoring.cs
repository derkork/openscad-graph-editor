namespace OpenScadGraphEditor.Refactoring
{
    public class RenameInvokableRefactoring : Refactoring
    {
        private string _id;
        private string _oldName;
        private string _newName;

        public RenameInvokableRefactoring(string id, string oldName, string newName)
        {
            _id = id;
            _oldName = oldName;
            _newName = newName;
        }
    }

}