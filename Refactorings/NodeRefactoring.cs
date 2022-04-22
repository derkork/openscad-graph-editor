using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Base class for refactorings that apply on a single node.
    /// </summary>
    public abstract class NodeRefactoring : Refactoring
    {
        protected IScadGraph Holder { get; }
        protected ScadNode Node { get; }

        protected NodeRefactoring(IScadGraph holder, ScadNode node)
        {
            Holder = holder;
            Node = node;
        }
    }
}