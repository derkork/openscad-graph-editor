using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Utils;
using Serilog;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring that replaces all currently known external references with the new ones given. The refactoring
    /// tries to preserve as much as possible.
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
            var newReferences = LoadReferences(project, _newReferences).AllReferences.ToList();
            
            // now we need to fix up all references to the old references and see if we can find a match in the new references
            // first make some variables to cut down on the linq
            var modules = newReferences.SelectMany(it => it.Modules).ToList();
            var functions = newReferences.SelectMany(it => it.Functions).ToList();
            var variables = newReferences.SelectMany(it => it.Variables).ToList();
            
            // until now the project is still unchanged. We will first massage the old references to look like the new
            // ones and then just swap them out. This way we can use the already existing refactorings to do the work for us.
            // start with modules
            foreach (var (oldModule, nodes) in nodesReferencingModules)
            {
                // find the new module that matches best
                if (!modules.TryFindMatch(oldModule, out var newModule))
                {
                    // if we can't find a match we need to remove the nodes (for now, later we may introduce some orphaned node handling)
                    Log.Information("Could not find a match for module {module}, removing all references to it.", oldModule.Name);
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
            
            // now do the same for functions
            foreach (var (oldFunction, nodes) in nodesReferencingFunctions)
            {
                if (!functions.TryFindMatch(oldFunction, out var newFunction))
                {
                    Log.Information("Could not find a match for function {function}, removing all references to it.", oldFunction.Name);
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
            
            // finally the variables
            foreach (var (oldVariable, nodes) in nodesReferencingVariables)
            {
                if (!variables.TryFindMatch(oldVariable, out var newVariable))
                {
                    Log.Information("Could not find a match for variable {variable}, removing all references to it.", oldVariable.Name);
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
            
            // now we can just swap out the old references with the new ones
            oldReferences.ForAll(it => project.RemoveExternalReference(it));
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
        
           
        /// <summary>
        /// Adds a reference to a scad file to the project using the given include mode.
        /// </summary>
        private static ExternalUnit LoadReferences(ScadProject project, Dictionary<string,IncludeMode> references)
        {
            var result = new ExternalUnit(project);
            references.ForAll(it => LoadReference(result, project.ProjectPath, it.Key, it.Value));
            return result;
        }

        private static void LoadReference(ExternalUnit unit, string sourceFile, string includePath, IncludeMode mode,
            [CanBeNull] ExternalReference owner = null)
        {
            GdAssert.That(owner == null || mode == IncludeMode.Include, "We should not have include mode 'use' for a transitive reference.");
            var resolved = PathResolver.TryResolve(sourceFile, includePath, out var fullPath);
            GdAssert.That(resolved, $"Could not resolve reference '{sourceFile}'");
            
            // now check if we have any external reference with the same path in the unit already
            if (unit.HasPath(fullPath))
            {
                // if so, we can skip this one.
                Log.Information("Reference to {FullPath} already loaded, skipping it", fullPath);
                return;
            }
            
            // build an external reference to parse into
            var externalReference = ExternalReferenceBuilder.Build(mode, includePath, owner);

            if (!ParseFile(externalReference, fullPath))
            {
                return;
            }

            // and add it to the project
            unit.AddReferenceIfNotExists(externalReference, fullPath);
                
                
            // now recursively add all transitive references
            foreach (var transitiveReference in externalReference.References)
            {
                LoadReference(unit, fullPath, transitiveReference, IncludeMode.Include, externalReference);
            }
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