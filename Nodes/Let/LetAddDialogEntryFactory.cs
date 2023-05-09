using System.Collections.Generic;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Nodes.Let
{
    [UsedImplicitly]
    public class LetAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(IEditorContext editorContext)
        {
            return new List<IAddDialogEntry>
            {
                // add an entry for the let expression
                new BoundPairBasedEntry(Resources.ScadBuiltinIcon, "Let (expression) [Ltex]",
                    "let, variable",
                    NodeFactory.Build<LetExpressionStart>,
                    NodeFactory.Build<LetExpressionEnd>,
                    editorContext),
                
                // and one for the let block
                new BoundPairBasedEntry(Resources.ScadBuiltinIcon, "Let (block) [Ltbl]",
                    "let, variable",
                    NodeFactory.Build<LetBlockStart>,
                    NodeFactory.Build<LetBlockEnd>,
                    editorContext)
            };
        }
    }
}