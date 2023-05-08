using System.Linq;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Nodes.Sum
{
    /// <summary>
    /// Refactoring for the sum node.
    /// </summary>
    public class DeleteUnassignableConnectionsRefactoring : NodeRefactoring
    {
        private readonly PortType _targetPortType;

        public DeleteUnassignableConnectionsRefactoring(ScadGraph owner, ScadNode node, PortType targetPortType) : base(owner, node)
        {
            GdAssert.That(targetPortType.IsExpressionType(), "target port type must be an expression type");
            _targetPortType = targetPortType;
        }

        public override bool IsLate => true;

        public override void PerformRefactoring(RefactoringContext context)
        {
            // find all connections which have an originating port type
            // which is not assignable to the target port type
            
            var connections = Holder
                .GetAllConnections()
                .Where(it => ScadConnectionExt.IsTo(it, Node, 0))
                .Where(it => it.TryGetFromPortType(out var portType) && !portType.CanBeAssignedTo(_targetPortType))
                .ToList();
          
            
            // delete them
            connections.ForAll(it => context.PerformRefactoring(new DeleteConnectionRefactoring(it)));
        }
    }
}