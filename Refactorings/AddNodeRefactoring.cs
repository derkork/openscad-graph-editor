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
        private readonly int _otherPort;
        private readonly bool _isIncoming;

        public AddNodeRefactoring(IScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public AddNodeRefactoring(IScadGraph holder, ScadNode node, [CanBeNull] ScadNode other, int otherPort, bool isIncoming) : base(holder, node)
        {
            _other = other;
            _otherPort = otherPort;
            _isIncoming = isIncoming;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var graph = context.MakeRefactorable(Holder);
            graph.AddNode(Node);
            
            if (_other != null)
            {
                var otherNode = graph.ById(_other.Id);
                if (_isIncoming)
                {
                    // try build a connection from the other node to an input port of the new node.
                    for (var i = 0; i < Node.InputPortCount; i++)
                    {
                        var connection = new ScadConnection(graph, otherNode, _otherPort, Node,  i);
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
                        var connection = new ScadConnection(graph, Node, i, otherNode, _otherPort);
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