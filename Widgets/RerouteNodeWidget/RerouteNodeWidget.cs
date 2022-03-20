using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets.RerouteNodeWidget
{
    public class RerouteNodeWidget : ScadNodeWidget
    {
        public override void BindTo(ScadNode node)
        {
            BoundNode = node;
        }
    }
}