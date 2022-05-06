using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Adds a new node to the graph. Has a variant where you can give a second node to connect to.
    /// Will automatically add a connection for this node.
    /// </summary>
    public class AddNodeRefactoring : NodeRefactoring
    {
        [CanBeNull]
        private readonly ScadNode _other;
        private readonly PortId _otherPort;

        public AddNodeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public AddNodeRefactoring(IScadGraph holder, ScadNode node, [CanBeNull] ScadNode other, PortId otherPort) : base(holder, node)
        {
            _otherPort = otherPort;
            GdAssert.That(other == null || otherPort.IsDefined, "otherPort must be defined when a node is given");
            _other = other;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            Holder.AddNode(Node);
            
            if (_other != null)
            {
                // TODO: we could simplify this if we use PortIds all the way.
                if (_otherPort.IsOutput)
                {
                    // try build a connection from the other node to an input port of the new node.
                    for (var i = 0; i < Node.InputPortCount; i++)
                    {
                        var connection = new ScadConnection(Holder, _other, _otherPort.Port, Node,  i);
                        if (ConnectionRules.CanConnect(connection).Decision == ConnectionRules.OperationRuleDecision.Allow)
                        {
                            context.PerformRefactoring(new AddConnectionRefactoring(connection));
                            break; // only create the first connection
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < Node.OutputPortCount; i++)
                    {
                        var connection = new ScadConnection(Holder, Node, i, _other, _otherPort.Port);
                        if (ConnectionRules.CanConnect(connection).Decision == ConnectionRules.OperationRuleDecision.Allow)
                        {
                            context.PerformRefactoring(new AddConnectionRefactoring(connection));
                            break; // only create the first connection
                        }
                    }
                }
            }
        }
    }
}