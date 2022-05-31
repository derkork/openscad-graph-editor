using System.Collections.Generic;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// An interface for factories that can produce <see cref="UserSelectableNodeRefactoring"/> instances for a given node.
    /// </summary>
    public interface IUserSelectableRefactoringFactory
    {
        IEnumerable<UserSelectableNodeRefactoring> GetRefactorings(ScadGraph graph, ScadNode node);
    }
}