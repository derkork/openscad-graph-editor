using Godot;

namespace OpenScadGraphEditor.Widgets.ScadItemList
{
    public class ScadItemListDragData : Reference
    {
        private readonly Widgets.ScadItemList.ScadItemList _sourceItemList;
        private readonly int _sourceItemIndex;

        public ScadItemListEntry Entry => _sourceItemList.GetEntry(_sourceItemIndex);

        public ScadItemListDragData(Widgets.ScadItemList.ScadItemList sourceItemList, int sourceItemIndex)
        {
            _sourceItemList = sourceItemList;
            _sourceItemIndex = sourceItemIndex;
        }
        

    }
}