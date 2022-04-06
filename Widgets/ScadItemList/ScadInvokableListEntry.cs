using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Widgets.ScadItemList
{
    public class ScadInvokableListEntry : ScadItemListEntry<InvokableDescription>
    {
        public ScadInvokableListEntry(InvokableDescription description, bool canBeDragged) : base(description, canBeDragged)
        {
        }

        public override string Title => Description.Name;
    }
}