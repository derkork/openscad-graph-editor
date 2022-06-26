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

        public AddNodeRefactoring(ScadGraph holder, ScadNode node) : base(holder, node)
        {
        }
        
        public AddNodeRefactoring(ScadGraph holder, ScadNode node, [CanBeNull] ScadNode other, PortId otherPort) : base(holder, node)
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
                // check if we can make a connection between the two nodes
                
                if (_otherPort.IsOutput)
                {
                    if (ConnectionRules.TryGetPossibleConnection(Holder, _other, Node, _otherPort, out var connection))
                    {
                        context.PerformRefactoring(new AddConnectionRefactoring(connection));
                    }
                }
                else if (_otherPort.IsInput)
                {
                    if (ConnectionRules.TryGetPossibleConnection(Holder,  Node, _other, _otherPort, out var connection))
                    {
                        context.PerformRefactoring(new AddConnectionRefactoring(connection));
                    }
                }
            }

            // if the user added a node that implies that the module can have children, set the "SupportsChildren" flag.
            if (Holder.Description is ModuleDescription moduleDescription && Node is IImplyChildren)
            {
                context.PerformRefactoring(new EnableChildrenRefactoring(moduleDescription));
            } 
        }
    }
}