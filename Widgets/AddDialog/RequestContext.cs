using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    public class RequestContext
    {

        public static RequestContext ForPosition(ScadGraphEdit source, Vector2 position)
        {
            return new RequestContext(source, position);
        }

        public static RequestContext ForPort(ScadGraphEdit source, Vector2 position, ScadNode node, PortId portId)
        {
            return new RequestContext(source, position, node, portId);
        }
        
        public static RequestContext FromPort(ScadGraphEdit source, Vector2 position, ScadNode node, int port)
        {
            return ForPort(source, position, node, PortId.Output(port));
        }
        public static RequestContext ToPort(ScadGraphEdit source, Vector2 position, ScadNode node, int port)
        {
            return ForPort(source, position, node, PortId.Input(port));
        }

        public static RequestContext ForNode(ScadGraphEdit source, Vector2 position, ScadNode node)
        {
            return new RequestContext(source, position, node);
        }

        private RequestContext(ScadGraphEdit source, Vector2 position, ScadNode node = null, PortId portId = default )
        {
            Source = source;
            Position = position;
            _node = node;
            _port = portId;
        }

        public ScadGraphEdit Source { get; }
        public Vector2 Position { get; }

        private readonly ScadNode _node;
        private readonly PortId _port;

        public bool TryGetNodeAndPort(out ScadNode node, out PortId port)
        {
            if (_node == null || !_port.IsDefined)
            {
                node = null;
                port = default;
                return false;
            }

            node = _node;
            port = _port;
            return true;
        }

        public bool TryGetNode(out ScadNode node)
        {
            if (_node == null)
            {
                node = default;
                return false;
            }

            node = _node;
            return true;
        }
        
    }
}