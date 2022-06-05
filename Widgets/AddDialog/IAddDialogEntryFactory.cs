using System.Collections.Generic;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    /// <summary>
    /// Factory interface for factories which want to produce entries for the <see cref="AddDialog"/>.
    /// </summary>
    public interface IAddDialogEntryFactory
    {
        IEnumerable<IAddDialogEntry> BuildEntries(ScadProject currentProject, ICanPerformRefactorings canPerformRefactorings);
    }
}