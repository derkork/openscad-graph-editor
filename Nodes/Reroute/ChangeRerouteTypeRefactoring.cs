using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Nodes.Reroute
{
    public class ChangeRerouteTypeRefactoring : NodeRefactoring
    {
        private readonly PortType _newConnectionType;

        public ChangeRerouteTypeRefactoring(ScadGraph holder, ScadNode node, PortType newConnectionType) : base(holder, node)
        {
            _newConnectionType = newConnectionType;
        }

        public override void PerformRefactoring(RefactoringContext context)
        {
            var currentConnectionType = Node.GetPortType(PortId.Input(0));
            if (currentConnectionType == _newConnectionType)
            {
                return; // nothing to do
            }

            // update the port type
            ((RerouteNode) Node).UpdatePortType(_newConnectionType);
          
        }
    }
}