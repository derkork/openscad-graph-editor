using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// This factory adds entries to the "Add Dialog" which represent built-in functionality.
    /// </summary>
    [UsedImplicitly]
    public class BuiltinsAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(IEditorContext editorContext)
        {

            var result = new List<IAddDialogEntry>();
            
            // built-in language level nodes (operators, etc.)
            result.AddRange(
                BuiltIns.LanguageLevelNodes
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.ScadBuiltinIcon,
                        () => NodeFactory.Build(it),
                        editorContext
                    ))
            );

            // built-in functions
            result.AddRange(
                BuiltIns.Functions
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.FunctionIcon,
                        () => NodeFactory.Build<FunctionInvocation>(it),
                        editorContext
                    ))
            );

            // built-in modules
            result.AddRange(
                BuiltIns.Modules
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.ModuleIcon,
                        () => NodeFactory.Build<ModuleInvocation>(it),
                        editorContext
                    )));
            
            // setters + getters for built-in variables
            // for the variables directly in the project
            result.AddRange(
                BuiltIns.Variables
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.VariableIcon,
                        () => NodeFactory.Build<SetVariable>(it),
                        editorContext
                    ))
            );

            result.AddRange(
                BuiltIns.Variables
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.VariableIcon,
                        () => NodeFactory.Build<GetVariable>(it),
                        editorContext
                    ))
            );

            return result;
        }
    }
}