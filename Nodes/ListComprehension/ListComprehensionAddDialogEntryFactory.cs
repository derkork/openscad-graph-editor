using System.Collections.Generic;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Nodes.ListComprehension
{
    public class ListComprehensionAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(ScadProject currentProject,
            ICanPerformRefactorings canPerformRefactorings)
        {
            // add an entry for the for-comprehension loop
            yield return new BoundPairBasedEntry(Resources.ScadBuiltinIcon, "For-Comprehension [Foco]",
                "for-comprehension, map",
                NodeFactory.Build<ForComprehensionStart>,
                NodeFactory.Build<ForComprehensionEnd>,
                canPerformRefactorings);
        }
    }
}