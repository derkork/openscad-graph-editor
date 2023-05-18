using System;
using System.Collections.Generic;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Widgets;

namespace OpenScadGraphEditor.Actions
{
    /// <summary>
    /// This inter
    /// </summary>
    public interface IEditorContext
    {
        /// <summary>
        /// The currently open project.
        /// </summary>
        ScadProject CurrentProject { get; }
        
        /// <summary>
        /// The editor configuration.
        /// </summary>
        Configuration Configuration { get; }
        
        /// <summary>
        /// Opens the refactor dialog for the given invokable description.
        /// </summary>
        void OpenRefactorDialog(InvokableDescription invokableDescription);
        
        /// <summary>
        /// Opens the refactor dialog for the given variable description.
        /// </summary>
        void OpenRefactorDialog(VariableDescription variableDescription);
       
        /// <summary>
        /// Opens the refactor dialog for the given external reference.
        /// </summary>
        /// <param name="externalReference"></param>
        void OpenRefactorDialog(ExternalReference externalReference);
        
        /// <summary>
        /// Opens the documentation dialog for the given invokable description.
        /// </summary>
        void OpenDocumentationDialog(InvokableDescription invokableDescription);
        
        /// <summary>
        /// Performs a refactoring with the given description.
        /// </summary>
        RefactoringData PerformRefactoring(string description, Refactoring refactoring);

        /// <summary>
        /// Performs the given refactorings as a block operation.
        /// </summary>
        RefactoringData PerformRefactorings(string title, IEnumerable<Refactoring> refactorings);
        
        /// <summary>
        /// Finds all usages of the given invokable description and shows them in the editor.
        /// </summary>
        void FindAndShowUsages(InvokableDescription invokableDescription);

        /// <summary>
        /// Finds all usages of the given variable description and shows them in the editor.
        /// </summary>
        void FindAndShowUsages(VariableDescription variableDescription);
        
        /// <summary>
        /// Opens the graph for the given invokable description. Focuses on the entry point.
        /// </summary>
        void OpenGraph(InvokableDescription invokableDescription);

        /// <summary>
        /// Opens the comment editing dialog for the given node.
        /// </summary>
        void EditComment(ScadGraph graph, ScadNode node);

        /// <summary>
        /// Opens the node color editing dialog for the given node.
        /// </summary>
        void EditNodeColor(ScadGraph graph, ScadNode node);
        
        /// <summary>
        /// Shows the help for the given node.
        /// </summary>
        void ShowHelp(ScadGraph graph, ScadNode node);

        /// <summary>
        /// Shows the given message as a popup info in the bottom right.
        /// </summary>
        void ShowInfo(string message);
        
        /// <summary>
        /// Copies the given nodes to the editor node clipboard.
        /// </summary>
        void CopyNodesToClipboard(ScadGraph graph, IEnumerable<ScadNode> selection);

        /// <summary>
        /// Shows the popup menu for the item represented by the given request context.
        /// </summary>
        void ShowPopupMenu(RequestContext requestContext);

        /// <summary>
        /// Shows the popup menu with the given entries at the given position.
        /// </summary>
        void ShowPopupMenu(Vector2 position, List<QuickAction> entries);
        
        /// <summary>
        /// Returns the nodes in the editor node clipboard as a graph.
        /// </summary>
        ScadGraph GetNodesInClipboard();

        /// <summary>
        /// Opens the add node dialog for the given request context.
        /// </summary>
        void OpenAddDialog(RequestContext requestContext);
    }
}