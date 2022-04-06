using OpenScadGraphEditor.Library;

namespace OpenScadGraphEditor.Widgets.ScadItemList
{
    public class ScadVariableListEntry : ScadItemListEntry<VariableDescription>
    {
        public ScadVariableListEntry(VariableDescription description) : base(description, true)
        {
        }

        public override string Title => Description.Name;
    }
}