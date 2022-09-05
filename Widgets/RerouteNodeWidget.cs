using System.Linq;
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
        public override bool RenderTitle => false;

        private ScadGraph _boundGraph;

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

        public override void BindTo(ScadGraph graph, ScadNode node)
        {
            BoundNode = node;
            Offset = node.Offset;
            _boundGraph = graph;
            RefreshType();
            if (_portType.IsExpressionType())
            {
                HintTooltip = ((RerouteNode) node).Render(graph, 0);
            }
        }

        protected override bool TryGetComment(out string comment)
        {
            comment = default;

            if (_boundGraph == null || BoundNode == null)
            {
                return false;
            }

            if (BoundNode.TryGetComment(out comment))
            {
                return true; // if the node has a manual comment, use this one.
            }

            var referenceNode = BoundNode;
            
            // try to find a predecessor that has a comment
            // we will skip over reroute nodes that have no comment until we reach a non-reroute node.
            do
            {
                var predecessor = _boundGraph.GetAllConnections().FirstOrDefault(it => it.To == referenceNode)?.From;
                if (predecessor == null)
                {
                    return false;
                }

                if (predecessor.TryGetComment(out comment))
                {
                    return true;
                }

                if (predecessor is RerouteNode)
                {
                    // try again
                    referenceNode = predecessor;
                }
                else
                {
                    return false; 
                }
            } while (true);
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