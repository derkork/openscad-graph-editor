using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Library.External;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Actions
{
    /// <summary>
    /// This inter
    /// </summary>
    public interface IEditorContext
    {
        ScadProject CurrentProject { get; }
        
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
        void PerformRefactoring(string description, Refactoring refactoring);

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

    }
}