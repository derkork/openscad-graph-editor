using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes
{
    /// <summary>
    /// Interface for node types which require special destruction code to be run.
    /// </summary>
    public interface IHaveSpecialDestruction
    {
        Refactoring GetDestructionRefactoring(IScadGraph nodeHolder);
    }
}