using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets;
using Serilog;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring that replaces all currently known external references with the new ones given. The refactoring
    /// tries to preserve as much as possible. This is one of the most complex refactorings in the whole project,
    /// and it probably still has bugs.
    /// </summary>
    public class ReplaceExternalReferencesRefactoring : Refactoring
    {
        private readonly Dictionary<string, IncludeMode> _newReferences;

        public ReplaceExternalReferencesRefactoring(Dictionary<string, IncludeMode> newReferences)
        {
            _newReferences = newReferences;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var project = context.Project;
            var oldReferences = project.ExternalReferences.ToList();
            // make a list of all nodes in the project which refer to modules from the old references
            var nodesReferencingModules = oldReferences
                // get the modules
                .SelectMany(it => it.Modules)
                .Select(it => (Module:it, Nodes:project.FindAllReferencingNodes(it).ToList()))
                .Where(it => it.Nodes.Any())
                .ToList();
            
            // same for functions
            var nodesReferencingFunctions = oldReferences
                .SelectMany(it => it.Functions)
                .Select(it => (Function:it, Nodes:project.FindAllReferencingNodes(it).ToList()))
                .Where(it => it.Nodes.Any())
                .ToList();
            
            // same for variables
            var nodesReferencingVariables = oldReferences
                .SelectMany(it => it.Variables)
                .Select(it => (Variable:it, Nodes:project.FindAllReferencingNodes(it).ToList()))
                .Where(it => it.Nodes.Any())
                .ToList();
            
            // load the new references into a unit
            var externalUnit = new ExternalUnit(project);
            foreach (var newReference in _newReferences)
            {
                var result = TryLoadReference(externalUnit, project.ProjectPath, newReference.Key, newReference.Value);
                // this is a top level reference. If it failed, the file may have moved elsewhere, so we try to keep
                // the information we have cached about this reference. If the file reappears later, we can just
                // update it then but for now we keep all the information we have so we don't delete stuff from our graphs
                // just because the file is missing

                if (result != LoadReferenceStatus.Failed)
                {
                    // all good, we can continue
                    continue;
                }

                // try to find the matching top-level reference in the old references
                var matchingOldReference = project.ExternalReferences
                    .FirstOrDefault(it =>
                        it.IncludePath == newReference.Key && it.Mode == newReference.Value && !it.IsTransitive);

                if (matchingOldReference != null) // we found a match
                {
                    // put this into the new unit
                    // strictly speaking the path is not the full path, but the path relative to the project, but
                    // since we don't actually have the file anymore, so we cannot resolve a full path
                    externalUnit.AddReferenceIfNotExists(matchingOldReference, newReference.Key);
                    
                    // issue a warning to the user
                    NotificationService.ShowError($"Could not load external reference {newReference.Key}, the file does not exist. I'm keeping the old reference for now, but the project will likely not work right now. Please fix the path to that file.");
                    
                    // also, add all the transitive references of the old reference to the new unit
                    var transitiveReferences = project.ExternalReferences
                        .Where(it => it.IsTransitive && it.IncludedBy == matchingOldReference.Id);
                        
                    foreach (var transitiveReference in transitiveReferences)
                    {
                        externalUnit.AddReferenceIfNotExists(transitiveReference, transitiveReference.IncludePath);
                    }
                }
                else
                {
                    // no match found, so somehow the user got an invalid path into the project. We will just
                    // ignore this reference and hope for the best
                    NotificationService.ShowError($"Could not load external reference {newReference.Key}, the file does not exist. I'm ignoring this reference.");
                }
            }
            
            var newReferences = externalUnit.AllReferences.ToList();
            
            // now we need to fix up all references to the old references and see if we can find a match in the new references
            // first make some variables to cut down on the linq
            var modules = newReferences.SelectMany(it => it.Modules).ToList();
            var functions = newReferences.SelectMany(it => it.Functions).ToList();
            var variables = newReferences.SelectMany(it => it.Variables).ToList();
            
            // until now the project is still unchanged. We will first massage the old references to look like the new
            // ones and then just swap them out. This way we can use the already existing refactorings to do the work for us.
            // start with modules
            
            // if we delete nodes, we need to keep track of which ones we deleted so we don't try to fix them up later
            var goneModules = new HashSet<ModuleDescription>();
            foreach (var (oldModule, nodes) in nodesReferencingModules)
            {
                // find the new module that matches best
                if (!modules.TryFindMatch(oldModule, out var newModule))
                {
                    // if we can't find a match we need to remove the nodes (for now, later we may introduce some orphaned node handling)
                    Log.Information("Could not find a match for module {module}, removing all references to it.", oldModule.Name);
                    // we cannot modify the list while iterating over it, so we need to collect the modules first
                    goneModules.Add(oldModule);
                    foreach (var node in nodes)
                    {
                        context.PerformRefactoring(new DeleteNodeRefactoring(node.Graph, node.Node));
                    }
                    continue;
                }
                
                // we found a match now we need to correct the parameters and the child support feature
                FixParameters(context, oldModule, newModule);
                
                // if the old module and new module have different child support, we need to add or remove the child support
                if (oldModule.SupportsChildren == newModule.SupportsChildren)
                {
                    continue;
                }

                if (newModule.SupportsChildren)
                {
                    context.PerformRefactoring(new EnableChildrenRefactoring(oldModule));
                }
                else
                {
                    context.PerformRefactoring(new DisableChildrenRefactoring(oldModule));
                }
            }
            // drop all the modules we deleted
            nodesReferencingModules.RemoveAll(it => goneModules.Contains(it.Module));
            
            
            var goneFunctions = new HashSet<FunctionDescription>();
            // now do the same for functions
            foreach (var (oldFunction, nodes) in nodesReferencingFunctions)
            {
                if (!functions.TryFindMatch(oldFunction, out var newFunction))
                {
                    Log.Information("Could not find a match for function {function}, removing all references to it.", oldFunction.Name);
                    // same as above, we cannot modify the list while iterating over it
                    goneFunctions.Add(oldFunction);
                    foreach (var node in nodes)
                    {
                        context.PerformRefactoring(new DeleteNodeRefactoring(node.Graph, node.Node));
                    }
                    continue;
                }
                
                // we found a match now we need to correct the parameters
                FixParameters(context, oldFunction, newFunction);
                
                // we also need to fix the return type in case it changed
                if (oldFunction.ReturnTypeHint != newFunction.ReturnTypeHint)
                {
                    context.PerformRefactoring(new ChangeFunctionReturnTypeRefactoring(oldFunction, newFunction.ReturnTypeHint));
                }
            }
            // drop all the functions we deleted
            nodesReferencingFunctions.RemoveAll(it => goneFunctions.Contains(it.Function));
            
            var goneVariables = new HashSet<VariableDescription>();
            // finally the variables
            foreach (var (oldVariable, nodes) in nodesReferencingVariables)
            {
                if (!variables.TryFindMatch(oldVariable, out var newVariable))
                {
                    Log.Information("Could not find a match for variable {variable}, removing all references to it.", oldVariable.Name);
                    // same as above, we cannot modify the list while iterating over it
                    goneVariables.Add(oldVariable);
                    foreach (var node in nodes)
                    {
                        context.PerformRefactoring(new DeleteNodeRefactoring(node.Graph, node.Node));
                    }
                    continue;
                }
                
                // we found a match now we need to correct the type
                if (oldVariable.TypeHint != newVariable.TypeHint)
                {
                    context.PerformRefactoring(new ChangeVariableTypeRefactoring(oldVariable, newVariable.TypeHint));
                }
            }
            // drop all the variables we deleted
            nodesReferencingVariables.RemoveAll(it => goneVariables.Contains(it.Variable));
            
            // now we can just swap out the old references with the new ones
            // however we need to be careful, if an old reference is also a new reference, we don't want to remove it
            // and add it again, because that would break stuff. We also don't want to add it again, because that would
            // also break stuff. So we need to remove all old references that are also new references from both lists
            
            // so get the intersection of both lists
            var keepReferences = oldReferences.Intersect(newReferences).ToList();
            
            // and remove them from both lists
            oldReferences.RemoveAll(it => keepReferences.Contains(it));
            newReferences.RemoveAll(it => keepReferences.Contains(it));
            
            // now kill the remaining old references
            oldReferences.ForAll(it => project.RemoveExternalReference(it));
            // and add all new ones
            newReferences.ForAll(it => project.AddExternalReference(it));
            
            // and we need to update the references in the project
            foreach (var (oldModule, nodes) in nodesReferencingModules)
            {
                // find the new module that matches best
                modules.TryFindMatch(oldModule, out var newModule);
                GdAssert.That(newModule != null, "We should have found a match for the module.");
                nodes.ForAll(it => it.NodeAsReference.SetupPorts(newModule));
            }
            
            // now do the same for functions
            foreach (var (oldFunction, nodes) in nodesReferencingFunctions)
            {
                functions.TryFindMatch(oldFunction, out var newFunction);
                GdAssert.That(newFunction != null, "We should have found a match for the function.");
                nodes.ForAll(it => it.NodeAsReference.SetupPorts(newFunction));
            }
            
            // finally the variables
            foreach (var (oldVariable, nodes) in nodesReferencingVariables)
            {
                variables.TryFindMatch(oldVariable, out var newVariable);
                GdAssert.That(newVariable != null, "We should have found a match for the variable.");
                nodes.ForAll(it => it.NodeAsReference.SetupPorts(newVariable));
            }

        }

        private static void FixParameters(RefactoringContext context, InvokableDescription oldInvokable, InvokableDescription newInvokable)
        {
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

        }

        enum LoadReferenceStatus
        {
            Failed,
            Success,
            Skipped
        }

        private static LoadReferenceStatus TryLoadReference(ExternalUnit unit, string sourceFile, string includePath, IncludeMode mode,
            [CanBeNull] ExternalReference owner = null)
        {
            var resolved = PathResolver.TryResolve(sourceFile, includePath, out var fullPath);
            // if we cannot resolve the file, then it's a failure.
            if (!resolved)
            {
                return LoadReferenceStatus.Failed;
            }
            
            // now check if we have any external reference with the same path in the unit already
            if (unit.HasPath(fullPath))
            {
                // if so, we can skip this one.
                Log.Information("Reference to {FullPath} already loaded, skipping it", fullPath);
                return LoadReferenceStatus.Skipped;
            }
            
            // build an external reference to parse into
            var externalReference = ExternalReferenceBuilder.Build(mode, includePath, owner);

            if (!ParseFile(externalReference, fullPath))
            {
                Log.Warning("Failed to parse file {FullPath}", fullPath);
                return LoadReferenceStatus.Failed;
            }

            // and add it to the project
            unit.AddReferenceIfNotExists(externalReference, fullPath);
                
                
            // now recursively add all transitive references
            foreach (var transitiveReference in externalReference.References)
            {
                // for recursive references, we do best effort. If we cannot load a reference, we just skip it.
                TryLoadReference(unit, fullPath, transitiveReference, IncludeMode.Include, externalReference);
            }

            return LoadReferenceStatus.Success;
        }

        private static bool ParseFile(ExternalReference externalReference, string fullPath)
        {
            GdAssert.That(!externalReference.IsLoaded, "External reference is already loaded.");
            
            string text;
            try
            {
                // read text from file
                text = System.IO.File.ReadAllText(fullPath, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Log.Warning(e,"Cannot read file {Path}", fullPath);
                return false;
            }

            try
            {
                Log.Information("Parsing SCAD file {Path} ", fullPath);
                // parse contents and fill the reference with data
                ExternalFileParser.Parse(text, externalReference);
            }
            catch (Exception e)
            {
                Log.Warning(e,"Cannot parse file {Path}", fullPath);
                return false;
            }

            return true;
        }
    }
}