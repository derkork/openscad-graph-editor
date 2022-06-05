using System.Collections.Generic;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes.ListComprehension;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Nodes.Let
{
    public class LetAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(ScadProject currentProject,
            ICanPerformRefactorings canPerformRefactorings)
        {
            var result = new List<IAddDialogEntry>();

            // add an entry for the let expression
            result.Add(new BoundPairBasedEntry(Resources.ScadBuiltinIcon, "Let (expression)",
                "let, variable",
                NodeFactory.Build<LetExpressionStart>,
                NodeFactory.Build<LetExpressionEnd>,
                canPerformRefactorings));
            
            // and one for the let block
            result.Add(new BoundPairBasedEntry(Resources.ScadBuiltinIcon, "Let (block)",
                "let, variable",
                NodeFactory.Build<LetBlockStart>,
                NodeFactory.Build<LetBlockEnd>,
                canPerformRefactorings));

            return result;
        }
    }
}