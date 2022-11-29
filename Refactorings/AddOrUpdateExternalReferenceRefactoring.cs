using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;
using Serilog;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring which refreshes code from an external reference.
    /// </summary>
    public class AddOrUpdateExternalReferenceRefactoring : Refactoring
    {
        private readonly string _includePath;
        private readonly IncludeMode _includeMode;
        private readonly ExternalReference _replaces;

        public AddOrUpdateExternalReferenceRefactoring(string includePath, IncludeMode includeMode, [CanBeNull] ExternalReference replaces = null)
        {
            _includePath = includePath;
            _includeMode = includeMode;
            _replaces = replaces;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var toReplace = _replaces;
            if (!PathResolver.TryResolve(context.Project.ProjectPath, _includePath, out var fullIncludePath))
            {
                NotificationService.ShowError("Cannot find file to include at " + _includePath);
                return;
            }
            
            // step 1: find all references that are dirtied by this refactoring
            var dirtyReferences = new HashSet<ExternalReference>();
            // if we replace an existing reference this is dirty
            if (_replaces != null)
            {
                toReplace = _replaces;
            }
            else
            {
                // also if the user has included a file that is already in the project (either directly or as transitive dependency)
                // this is dirty
                toReplace = context.Project.ExternalReferences
                    .FirstOrDefault(r => r.TryResolveFullPath(context.Project.ProjectPath, context.Project.ResolveExternalReference, out var existingPath) 
                                         && PathResolver.IsSamePath(fullIncludePath, existingPath));
     
            }
          
            if (toReplace != null)
            {
                dirtyReferences.Add(toReplace);
            }
            
            // also mark all transitive dependencies as dirty that are referenced by the dirty references
            dirtyReferences.UnionWith(dirtyReferences.SelectMany(it => context.Project.GetTransitiveReferences(it)));
            
            // now load the reference and all transitive dependencies
            var newReferences = new HashSet<ExternalReference>();
            DependencyExt.LoadReference(context.Project.ProjectPath, _includePath, _includeMode, newReferences);
            
            // now we need to compare the old and the new references.
            // we will delete all references that are not required by another reference in the project
            var keptReferences = new HashSet<ExternalReference>();
            foreach (var dirtyReference in dirtyReferences)
            {
                // toReplace must be != null here, otherwise there would not be any dirty references
                if (context.Project.IsThisReferenceUsedByOtherReferenceThan(dirtyReference, toReplace))
                {
                    keptReferences.Add(dirtyReference);
                }
                else
                {
                    // remove the reference from the project
                    context.Project.RemoveExternalReference(dirtyReference);
                }
            }
            
            // if we have remaining references, we need to check if they refer to the same file as a reference
            // in the new set. If so, we need to replace the old reference with the new one
            foreach (var keptReference in keptReferences)
            {
                // get the full path of the kept reference
                if (context.Project.TryResolveFullPath(keptReference, out var keptFullPath))
                {
                    // check if there is a new reference that refers to the same file
                    foreach (var newReference in newReferences)
                    {
                        // since the new references are not part of the project yet, we need to resolve the full path
                        // differently
                        if (newReference.TryResolveFullPath(context.Project.ProjectPath,
                                it => newReferences.FirstOrDefault(re => re.Id == it), out var newFullPath))
                        {
                            // if the paths are the same, we need to replace the kept reference with the new one
                            if (PathResolver.IsSamePath(keptFullPath, newFullPath))
                            {
                                // replace the kept reference with the new one
                                context.Project.Remov(keptReference, newReference);
                                // remove the new reference from the new references set
                                newReferences.Remove(newReference);
                                // we can stop searching for a replacement
                                break;
                            }
                        }
                    }
                }
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
            
            // finally we need to update the transitive references of this file.
            // todo, this doesnt handle the case where new imports are added or imports are removed.
            // or files are reorganized in general.
            context.Project.ExternalReferences.Where(it => it.IncludedBy == _reference.Id)
                .Select(it => new AddOrUpdateExternalReferenceRefactoring(it))
                .ToList()
                .ForAll(context.PerformRefactoring);
        }
        
    }
}