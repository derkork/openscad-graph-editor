using System.Linq;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Refactoring for nodes which implement <see cref="IHaveVariableInputSize"/>. This removes an input from the node
    /// and fixes all connections.
    /// </summary>
    [UsedImplicitly]
    public class RemoveVariableInputRefactoring : UserSelectableNodeRefactoring
    {
        public override string Title => ((IHaveVariableInputSize) Node).RemoveRefactoringTitle;
        public override int Order => 1;

        public override bool IsApplicableToNode => Node is IHaveVariableInputSize variableInputSize &&
                                                   variableInputSize.CurrentInputSize > 1;

        public RemoveVariableInputRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var asVariableInputSize = (IHaveVariableInputSize) Node;

            // when decreasing input port count, we loose connections (incoming and outgoing)

            // incoming connections
            Holder.GetAllConnections()
                .Where(it => it.IsTo(Node, asVariableInputSize.InputPortOffset + asVariableInputSize.CurrentInputSize - 1))
                .ToList() // make a new list, so we don't change the collection while iterating over it
                .ForAll(it => Holder.RemoveConnection(it));

            if (asVariableInputSize.OutputPortsMatchVariableInputs)
            {
                // outgoing connections
                Holder.GetAllConnections()
                    .Where(it => it.IsFrom(Node, asVariableInputSize.OutputPortOffset + asVariableInputSize.CurrentInputSize - 1))
                    .ToList() // make a new list, so we don't change the collection while iterating over it
                    .ForAll(it => Holder.RemoveConnection(it));
            }

            // and tell the node to kill the ports
            asVariableInputSize.RemoveVariableInputPort();
        }
    }
}