using System.Collections.Generic;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Nodes.ListComprehension
{
    [UsedImplicitly]
    public class ListComprehensionAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(IEditorContext editorContext)
        {
            // add an entry for the for-comprehension loop
            yield return new BoundPairBasedEntry(Resources.ScadBuiltinIcon, "For-Comprehension [Foco]",
                "for-comprehension, map",
                NodeFactory.Build<ForComprehensionStart>,
                NodeFactory.Build<ForComprehensionEnd>,
                editorContext);
        }
    }
}