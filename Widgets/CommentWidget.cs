using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class CommentWidget : ScadNodeWidget
    {
        private RichTextLabel _rtl;
        protected override Theme UseTheme => Resources.StandardNodeWidgetTheme;

        private bool _resizePending;
        
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
            // we will not immediately fire the resize event because resize is a continuous event
            // that happens multiple times per frame and is actually only finished when the user
            // releases the mouse button. Therefore we will just note down that a resize event has
            // started but wait to fire it until the user actually releases the mouse button
            _resizePending = true;
            RectMinSize = size;
            QueueSort();
        }

        public override void _GuiInput(InputEvent evt)
        {
            if (!_resizePending)
            {
                return;
            }
            
            // check if it was a mouse up event and if so fire the resize event
            if (!(evt is InputEventMouseButton mouseButton) || mouseButton.Pressed || mouseButton.ButtonIndex != (int)ButtonList.Left)
            {
                return;
            }
            _resizePending = false;
            RaiseSizeChanged(RectMinSize);

        }

        public override void BindTo(IScadGraph graph, ScadNode node)
        {
            BoundNode = node;
            Offset = node.Offset;
            Title = node.NodeTitle;
            RectMinSize = ((Comment) node).Size;
            _rtl.Text = node.NodeDescription;
        }
    }
}