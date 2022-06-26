using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Base class for refactorings that apply on a single node.
    /// </summary>
    public abstract class NodeRefactoring : Refactoring
    {
        protected ScadGraph Holder { get; }
        protected ScadNode Node { get; }

        protected NodeRefactoring(ScadGraph holder, ScadNode node)
        {
            Holder = holder;
            Node = node;
        }
    }
}