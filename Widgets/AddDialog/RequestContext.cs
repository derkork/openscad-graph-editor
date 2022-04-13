using Godot;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    public class RequestContext
    {

        public static RequestContext AtPosition(ScadGraphEdit source, Vector2 position)
        {
            return new RequestContext(source, position, null, null, 0);
        }

        public static RequestContext From(ScadGraphEdit source, Vector2 position, ScadNode node, int port)
        {
            return new RequestContext(source, position, node, null, port);
        }
        public static RequestContext To(ScadGraphEdit source, Vector2 position, ScadNode node, int port)
        {
            return new RequestContext(source, position, null, node, port);
        }

        private RequestContext(ScadGraphEdit source, Vector2 lastReleasePosition, ScadNode sourceNode, ScadNode destinationNode,
            int lastPort)
        {
            Source = source;
            LastReleasePosition = lastReleasePosition;
            SourceNode = sourceNode;
            DestinationNode = destinationNode;
            LastPort = lastPort;
        }

        public ScadGraphEdit Source { get; }
        public Vector2 LastReleasePosition { get; }
        public ScadNode SourceNode { get; }
        public ScadNode DestinationNode { get; }
        public int LastPort { get; }
    }
}