using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Interface for node types which require special construction code to be run.
    /// </summary>
    public interface IHaveSpecialConstruction
    {
        Refactoring GetConstructionRefactoring(IScadGraph nodeHolder);
    }
}