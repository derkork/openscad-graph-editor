using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class SmallNodeWidget : ScadNodeWidget
    {
        private Vector2 _portSize;
        protected override Theme UseTheme => Resources.SimpleNodeWidgetTheme;

        
        public override void _Ready()
        {
            base._Ready();
            RectMinSize = new Vector2(32, 32);
            _portSize = Theme.GetIcon("port", "GraphNode").GetSize();

        }


        public override void BindTo(IScadGraph graph, ScadNode node)
        {
            base.BindTo(graph, node);
            Title = ""; // hide title
            
            // make the background shine through a bit.
            this.GetChildNodes<Control>().ForAll(it => it.Modulate = new Color(1,1,1,0.9f));
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            
            if (what != NotificationDraw || BoundNode == null || !(BoundNode is IHaveNodeBackground background))
            {
                return;
            }

            var texture = background.NodeBackground;
            if (texture == null)
            {
                return;
            }

            var textureSize = texture.GetSize();
            var aspectRatio = textureSize.x / textureSize.y;

            // leave enough border for the ports
            var availableSize = RectSize - (_portSize * 1.2f);
            
            
            // fit the texture to the available size keeping the aspect ratio
            var size = new Vector2(Mathf.Min(availableSize.x, availableSize.y * aspectRatio), Mathf.Min(availableSize.y, availableSize.x / aspectRatio));
            
            // center the texture
            var pos = new Vector2(Mathf.Max(0, (RectSize.x - size.x) / 2), Mathf.Max(0, (RectSize.y - size.y) / 2));
            

            DrawTextureRect(texture, new Rect2(pos, size), false, new Color(1, 1,1, 0.5f));
        }
    }
}