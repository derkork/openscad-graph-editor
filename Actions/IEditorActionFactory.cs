using System.Collections.Generic;

namespace OpenScadGraphEditor.Actions
{
    /// <summary>
    /// Factory class for editor actions.
    /// </summary>
    public interface IEditorActionFactory
    {
        IEnumerable<IEditorAction> CreateActions();
    }
}