using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// This factory adds entries to the "Add Dialog" which represent built-in functionality.
    /// </summary>
    public class BuiltinsAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(ScadProject currentProject, ICanPerformRefactorings canPerformRefactorings)
        {

            var result = new List<IAddDialogEntry>();
            
            result.AddRange(
                BuiltIns.LanguageLevelNodes
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.ScadBuiltinIcon,
                        () => NodeFactory.Build(it),
                        canPerformRefactorings
                    ))
            );

            result.AddRange(
                BuiltIns.Functions
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.FunctionIcon,
                        () => NodeFactory.Build<FunctionInvocation>(it),
                        canPerformRefactorings
                    ))
            );

            result.AddRange(
                BuiltIns.Modules
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.ModuleIcon,
                        () => NodeFactory.Build<ModuleInvocation>(it),
                        canPerformRefactorings
                    )));

            return result;
        }
    }
}