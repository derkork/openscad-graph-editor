using System;
using Godot;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    public interface IAddDialogEntry
    {
        /// <summary>
        /// The title that should be displayed.
        /// </summary>
        string Title { get; }
        
        /// <summary>
        /// Keywords that can be used to find the entry.
        /// </summary>
        string Keywords { get; }

        /// <summary>
        /// Action to be performed when this entry is selected.
        /// </summary>
        Action<RequestContext> Action { get; }

        /// <summary>
        /// The icon to use to represent this entry.
        /// </summary>
        Texture Icon { get; }

        /// <summary>
        /// Whether or not the entry can be added given the current context.
        /// </summary>
        EntryFittingDecision CanAdd(RequestContext context);
    }
}