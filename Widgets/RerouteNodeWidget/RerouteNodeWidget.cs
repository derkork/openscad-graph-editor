using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets.RerouteNodeWidget
{
    public class RerouteNodeWidget : ScadNodeWidget
    {
        public override void BindTo(ScadNode node)
        {
            BoundNode = node;
            Offset = node.Offset;
            RefreshType();
        }

        private void RefreshType()
        {
            var portType = BoundNode.GetInputPortType(0);
            SetSlotColorLeft(0, ColorFor(portType));
            SetSlotTypeLeft(0, (int) portType);
            SetSlotColorRight(0, ColorFor(portType));
            SetSlotTypeRight(0, (int) portType);
            
        }

        public override void PortConnected(int port, bool isLeft)
        {
            RefreshType();
        }

        public override void PortDisconnected(int port, bool isLeft)
        {
            RefreshType();
        }
    }
}