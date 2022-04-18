using Godot;

namespace OpenScadGraphEditor.Widgets.ProjectTree
{
    public class ProjectTreeDragData : Reference
    {
        private readonly ProjectTree _sourceItemList;
        private readonly string _id;

        public ProjectTreeEntry Entry => _sourceItemList.GetEntry(_id);

        public ProjectTreeDragData(ProjectTree sourceItemList, string id)
        {
            _sourceItemList = sourceItemList;
            _id = id;
        }
        

    }
}