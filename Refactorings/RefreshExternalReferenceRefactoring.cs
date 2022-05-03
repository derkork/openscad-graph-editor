using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring which refreshes code from an external reference.
    /// </summary>
    public class RefreshExternalReferenceRefactoring : Refactoring
    {
        private readonly ExternalReference _reference;

        public RefreshExternalReferenceRefactoring(ExternalReference reference)
        {
            _reference = reference;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            // make a new copy of the reference and then load the current file contents into it.
            var referenceCopy = ExternalReferenceBuilder.BuildEmptyCopy(_reference);
            if (!_reference.TryResolveFullPath(context.Project, out var fullPath))
            {
                GD.PrintErr("Could not resolve full path for external reference.");
                return;
            }

            if (!referenceCopy.ParseFile(fullPath))
            {
                GD.PrintErr($"Could not parse file at path: {fullPath}");
                return;
            }



            // now we have the full contents of the reference and can start to update our code.
            // the important things are:
            // 1. modules, functions and variables that were deleted from the original file. we need to clean up
            //    all nodes that would refer to these.
            // 2. modules and functions that were added. we need to add these to the list of known modules/functions
            //    for that file.
            // 3. modules and functions that had parameters changed. we need to update the parameters
            //    of all invocations of these.

            // 1 -  DELETE references to stuff that is no longer there.

            // make some helper variables so we can cut down on the linq
            var oldInvokables = _reference.Functions.Concat<InvokableDescription>(_reference.Modules).ToList();
            var newInvokables = referenceCopy.Functions.Concat<InvokableDescription>(referenceCopy.Modules).ToList();

            // first start with the deletions. delete all invokables that exist in the copy but not in the original.
            oldInvokables
                .Where(oldInvokable => newInvokables.All(newInvokable => newInvokable.Id != oldInvokable.Id))
                .Select(it => new DeleteInvokableRefactoring(it))
                .ToList()
                .ForAll(context.PerformRefactoring);

            // and variables
            _reference.Variables
                .Where(oldVariable => referenceCopy.Variables.All(newVariable => newVariable.Id != oldVariable.Id))
                .Select(it => new DeleteVariableRefactoring(it))
                .ToList()
                .ForAll(context.PerformRefactoring);

            // 2 - ADD references to stuff that is new.
            // this is very easy as no graphs need to be updated for this, we can just add this stuff.
            
            // new functions
            referenceCopy.Functions
                .Where(newFunction => _reference.Functions.All(oldFunction => oldFunction.Id != newFunction.Id))
                .ToList()
                .ForAll(_reference.Functions.Add);
            // new modules
            referenceCopy.Modules
                .Where(newModule => _reference.Modules.All(oldModule => oldModule.Id != newModule.Id))
                .ToList()
                .ForAll(_reference.Modules.Add);
            // new variables
            referenceCopy.Variables
                .Where(newVariable => _reference.Variables.All(oldVariable => oldVariable.Id != newVariable.Id))
                .ToList()
                .ForAll(_reference.Variables.Add);
            
            // 3 - CHANGE Modules and functions that had parameters changed.
            // for now we can keep this rather simple as we don't look at parameter types for external references,
            // simply because there is no explicit way to specify a parameter type in OpenScad anyways.

            // now we should be able to make pairs of old and new invokables as we should have a new one
            // for each old now.
            var oldToNewInvokables = new Dictionary<InvokableDescription, InvokableDescription>();
            _reference.Functions.ForAll(it =>
                oldToNewInvokables[it] = referenceCopy.Functions.First(newIt => newIt.Id == it.Id));
            _reference.Modules.ForAll(it =>
                oldToNewInvokables[it] = referenceCopy.Modules.First(newIt => newIt.Id == it.Id));

            foreach (var pair in oldToNewInvokables)
            {
                var oldInvokable = pair.Key;
                var newInvokable = pair.Value;

                // detecting whether a parameter was added, renamed or deleted is somewhat of a heuristic. The things we can go
                // by are the position of the parameter and the name. If the name is the same we can assume
                // it is the same parameter (even if it changes position). If the name is different
                // it could be either a new parameter (and the old one was deleted) or a parameter that was renamed.
                // Since whatever we do will be wrong one way or the other we will go with this (as this is the simplest)
                // for now:
                // 1. if we don't find an old parameter in the new list anymore, we assume it is deleted
                // 2. if we don't find a new parameter in the old list, we assume it is added
                // 3. reorder the parameters so they match the new state.
                //
                // this implies we do not support renames, because we simply don't have the data to properly tell 
                // a rename apart from a delete and add.

                // step 1 - find the indices of all parameters that were deleted
                var removedParameterIndices = oldInvokable.Parameters
                    .Select((it, index) => new {it, index})
                    .Where(it => newInvokable.Parameters.All(newIt => newIt.Name != it.it.Name))
                    .Select(it => it.index)
                    .ToArray();

                if (removedParameterIndices.Length > 0)
                {
                    context.PerformRefactoring(
                        new DeleteInvokableParametersRefactoring(oldInvokable, removedParameterIndices));
                }

                // step 2 - find and add added parameters.
                var addedParameters
                    = newInvokable.Parameters.Where(it => oldInvokable.Parameters.All(oldIt => oldIt.Name != it.Name))
                        .ToArray();

                if (addedParameters.Length > 0)
                {
                    context.PerformRefactoring(new AddInvokableParametersRefactoring(oldInvokable, addedParameters));
                }
                
                // step 3 - reorder the parameters so they match the new state.
                var newParameterNames = newInvokable.Parameters.Select(it => it.Name)
                    .ToArray();
                context.PerformRefactoring(new ChangeParameterOrderRefactoring(oldInvokable, newParameterNames));

                // we can ignore function return types for now as all types for external references are ANY right now.
            }
        }
    }
}