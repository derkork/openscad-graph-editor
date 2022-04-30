using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class CommentWidget : ScadNodeWidget
    {
        private RichTextLabel _rtl;
        protected override Theme UseTheme => Resources.StandardNodeWidgetTheme;

        public override void _Ready()
        {
            Comment = true;
            Resizable = true;
            base._Ready();
            var mc = new MarginContainer();
            mc.MouseFilter = MouseFilterEnum.Ignore;
            mc.SizeFlagsVertical = (int)SizeFlags.ExpandFill;

            // set godot margin container to 10 margin
            mc.AddConstantOverride("margin_top", 10);
            mc.AddConstantOverride("margin_bottom", 10);
            mc.AddConstantOverride("margin_left", 10);
            mc.AddConstantOverride("margin_right", 10);

            _rtl = new RichTextLabel();
            _rtl.MouseFilter = MouseFilterEnum.Ignore;
            _rtl.MoveToNewParent(mc);
            mc.MoveToNewParent(this);

            RectMinSize = new Vector2(100, 100);
            this.Connect("resize_request")
                .To(this, nameof(OnResizeRequested));

        }

        
        private void OnResizeRequested(Vector2 size)
        {
            RectMinSize = size;
            QueueSort();
        }
        
        public override void BindTo(IScadGraph graph, ScadNode node)
        {
            BoundNode = node;
            Offset = node.Offset;
            Title = node.NodeTitle;
            _rtl.Text = node.NodeDescription;
        }
    }
}