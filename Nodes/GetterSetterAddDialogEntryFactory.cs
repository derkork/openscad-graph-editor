using System.Collections.Generic;
using System.Linq;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Factory which adds entries to get/set each variable defined in the project.
    /// </summary>
    public class GetterSetterAddDialogEntryFactory : IAddDialogEntryFactory
    {
        public IEnumerable<IAddDialogEntry> BuildEntries(ScadProject currentProject, ICanPerformRefactorings canPerformRefactorings)
        {
            var result = new List<IAddDialogEntry>();
            
            
            // for the variables directly in the project
            result.AddRange(
                currentProject.Variables
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.VariableIcon,
                        () => NodeFactory.Build<SetVariable>(it),
                        canPerformRefactorings
                    ))
            );

            result.AddRange(
                currentProject.Variables
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.VariableIcon,
                        () => NodeFactory.Build<GetVariable>(it),
                        canPerformRefactorings
                    ))
            );
            
            // for the variables in external modules
            var externalVariableDescriptions = currentProject.ExternalReferences
                .SelectMany(it => it.Variables)
                .ToList();

            result.AddRange(
                externalVariableDescriptions
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.VariableIcon,
                        () => NodeFactory.Build<SetVariable>(it),
                        canPerformRefactorings
                    ))
            );

            result.AddRange(
                externalVariableDescriptions
                    .Select(it => new SingleNodeBasedEntry(
                        Resources.VariableIcon,
                        () => NodeFactory.Build<GetVariable>(it),
                        canPerformRefactorings
                    ))
            );

            return result;
        }
    }
}