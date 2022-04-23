using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets.RerouteNodeWidget
{
    public class RerouteNodeWidget : ScadNodeWidget
    {
        public override void BindTo(IScadGraph graph, ScadNode node)
        {
            BoundNode = node;
            Offset = node.Offset;
            RefreshType();
        }

        private void RefreshType()
        {
            var portType = BoundNode.GetPortType(PortId.Input(0));
            SetSlotColorLeft(0, ColorFor(portType));
            SetSlotTypeLeft(0, (int) portType);
            SetSlotColorRight(0, ColorFor(portType));
            SetSlotTypeRight(0, (int) portType);
            
        }
    }
}