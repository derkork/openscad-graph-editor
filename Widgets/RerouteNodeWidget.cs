using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class RerouteNodeWidget : ScadNodeWidget
    {
        private PortType _portType;
        protected override Theme UseTheme => Resources.SimpleNodeWidgetTheme;

        public override void _Ready()
        {
            base._Ready();
            var box = new HBoxContainer();
            box.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
            box.MoveToNewParent(this);
            SetSize(new Vector2(48   ,32));
            RectMinSize = new Vector2(32, 32);
            SetSlotEnabledLeft(0, true);
            SetSlotEnabledRight(0, true);
            QueueSort();
        }

        public override void BindTo(IScadGraph graph, ScadNode node)
        {
            BoundNode = node;
            Offset = node.Offset;
            RefreshType();
            if (_portType.IsExpressionType())
            {
                HintTooltip = ((RerouteNode) node).RenderExpressionOutput(graph, 0);
            }
        }

        private void RefreshType()
        {
            _portType = BoundNode.GetPortType(PortId.Input(0));
            SetSlotColorLeft(0, ColorFor(_portType));
            SetSlotTypeLeft(0, (int) _portType);
            SetSlotColorRight(0, ColorFor(_portType));
            SetSlotTypeRight(0, (int) _portType);
            
        }
    }
}