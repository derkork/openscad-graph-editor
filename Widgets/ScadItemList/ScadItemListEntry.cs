namespace OpenScadGraphEditor.Widgets.ScadItemList
{
    public abstract class ScadItemListEntry
    {
        public abstract string Title { get; }
        public abstract bool CanBeDragged { get; }
    }
    
    public abstract class ScadItemListEntry<T> : ScadItemListEntry
    {
        public T Description { get; }
        public sealed override bool CanBeDragged { get; }

        protected ScadItemListEntry(T description, bool canBeDragged)
        {
            Description = description;
            CanBeDragged = canBeDragged;
        }
    }
}