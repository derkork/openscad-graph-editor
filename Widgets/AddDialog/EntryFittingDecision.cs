namespace OpenScadGraphEditor.Widgets.AddDialog
{
    /// <summary>
    /// A decision whether or not a node can be added to the graph and if it fits the context.
    /// </summary>
    public enum EntryFittingDecision
    {
        /// <summary>
        /// The node fits the context and can be added.
        /// </summary>
        Fits,

        /// <summary>
        /// The node does not fit the context but can be added anyway.
        /// </summary>
        DoesNotFit,

        /// <summary>
        /// The node does not fit the context and must not be added.
        /// </summary>
        Veto
    }
}