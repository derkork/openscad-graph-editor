using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class RerouteNodeWidget : SmallNodeWidget
    {
        private PortType _portType;
        protected override Theme UseTheme => Resources.SimpleNodeWidgetTheme;
        protected override bool RenderTitle => false;

        public override void _Ready()
        {
            base._Ready();
            var box = new HBoxContainer();
            box.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
            box.MoveToNewParent(this);
            RectMinSize = new Vector2(64, 64);
            SetSize(new Vector2(64   ,64));
            SetSlotEnabledLeft(0, true);
            SetSlotEnabledRight(0, true);
            QueueSort();
        }

        public override void BindTo(ScadProject project, ScadGraph graph, ScadNode node)
        {
            BoundNode = node;
            Offset = node.Offset;
            RefreshType();
            if (_portType.IsExpressionType())
            {
                HintTooltip = ((RerouteNode) node).Render(graph, 0);
            }
        }

        private void RefreshType()
        {
            _portType = BoundNode.GetPortType(PortId.Input(0));
            SetSlotColorLeft(0, _portType.Color());
            SetSlotTypeLeft(0, (int) _portType);
            SetSlotColorRight(0, _portType.Color());
            SetSlotTypeRight(0, (int) _portType);
            
        }
    }
}