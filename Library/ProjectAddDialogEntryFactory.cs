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
    /// This factory adds entries for all functions and modules that are described in the project and its imported
    /// libraries.
    /// </summary>
    [UsedImplicitly]
    public class ProjectAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(IEditorContext editorContext)
        {
            var result = new List<IAddDialogEntry>();

            // functions defined in this project
            result.AddRange(
                editorContext.CurrentProject.Functions
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.FunctionIcon,
                        () => NodeFactory.Build<FunctionInvocation>(it.Description),
                        editorContext
                    ))
            );

            // modules defined in this project
            result.AddRange(
                editorContext.CurrentProject.Modules
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.ModuleIcon,
                        () => NodeFactory.Build<ModuleInvocation>(it.Description),
                        editorContext
                    ))
            );
            
            // functions defined in external references
            result.AddRange(
                editorContext.CurrentProject.ExternalReferences
                    .SelectMany(it => it.Functions)
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.FunctionIcon,
                        () => NodeFactory.Build<FunctionInvocation>(it),
                        editorContext
                    ))
            );

            // modules defined in external references
            result.AddRange(
                editorContext.CurrentProject.ExternalReferences
                    .SelectMany(it => it.Modules)
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.ModuleIcon,
                        () => NodeFactory.Build<ModuleInvocation>(it),
                        editorContext
                    ))
                );
            
            return result;
        }
    }
}