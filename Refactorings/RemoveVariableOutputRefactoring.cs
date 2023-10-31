using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring for nodes which implement <see cref="IHaveVariableOutputSize"/>. This removes an output from the node
    /// and fixes all connections.
    /// </summary>
    [UsedImplicitly]
    public class RemoveVariableOutputRefactoring : NodeRefactoring
    {
        public RemoveVariableOutputRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            if (! (Node is IHaveVariableOutputSize asVariableOutputSize))
            {
                return; // not applicable
            }

            // when decreasing output port count, we loose outgoing connections

            // incoming connections
            Holder.GetAllConnections()
                .Where(it => it.IsFrom(Node, asVariableOutputSize.OutputPortOffset + asVariableOutputSize.CurrentOutputSize - 1))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => Holder.RemoveConnection(it));

       
            // and tell the node to kill the port
            asVariableOutputSize.RemoveVariableOutputPort();
        }
    }
}