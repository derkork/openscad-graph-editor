using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Library
{
    /// <summary>
    /// This factory adds entries for all functions and modules that are described in the project and its imported
    /// libraries.
    /// </summary>
    public class ProjectAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(ScadProject currentProject,
            ICanPerformRefactorings canPerformRefactorings)
        {
            var result = new List<IAddDialogEntry>();

            // functions defined in this project
            result.AddRange(
                currentProject.Functions
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.FunctionIcon,
                        () => NodeFactory.Build<FunctionInvocation>(it.Description),
                        canPerformRefactorings
                    ))
            );

            // modules defined in this project
            result.AddRange(
                currentProject.Modules
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.ModuleIcon,
                        () => NodeFactory.Build<ModuleInvocation>(it.Description),
                        canPerformRefactorings
                    ))
            );
            
            // functions defined in external references
            result.AddRange(
                currentProject.ExternalReferences
                    .SelectMany(it => it.Functions)
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.FunctionIcon,
                        () => NodeFactory.Build<FunctionInvocation>(it),
                        canPerformRefactorings
                    ))
            );

            // modules defined in external references
            result.AddRange(
                currentProject.ExternalReferences
                    .SelectMany(it => it.Modules)
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.ModuleIcon,
                        () => NodeFactory.Build<ModuleInvocation>(it),
                        canPerformRefactorings
                    ))
                );
            
            return result;
        }
    }
}