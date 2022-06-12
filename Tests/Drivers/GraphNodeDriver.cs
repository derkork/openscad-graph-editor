using System;
using System.Threading.Tasks;
using Godot;
using GodotTestDriver.Drivers;
using GodotTestDriver.Input;
using GodotTestDriver.Util;

namespace OpenScadGraphEditor.Tests.Drivers
{
    public class GraphNodeDriver : ControlDriver<GraphNode>
    {
        public GraphNodeDriver(Func<GraphNode> producer) : base(producer)
        {
        }

        public string Title => Root?.Title;
        public Vector2 Offset => Root?.Offset ?? Vector2.Inf;
        
        public bool Selected => Root?.Selected ?? false;

        /// <summary>
        /// Drags the node by the given amount of pixels.
        /// </summary>
        public async Task DragBy(Vector2 delta)
        {
            var node = PresentRoot;
            var rect = node.GetGlobalRect();

            // we assume that a position 5 pixels below the top border horizontally centered is a safe dragging
            // spot as the node will have a title there.
            var dragStart = rect.Position + new Vector2(rect.Size.x / 2, 5);
            var dragEnd = dragStart + delta;
            
            // drag it
            await Viewport.DragMouse(dragStart, dragEnd);
        }

        /// <summary>
        /// Drags the node by the given amount of pixels.
        /// </summary>
        public async Task DragBy(float x, float y)
        {
            await DragBy(new Vector2(x, y));
        }

        /// <summary>
        /// Drags the node by a multiple of its own size multiplied by the given factor.
        /// </summary>
        public async Task DragByOwnSize(Vector2 delta)
        {
            var node = PresentRoot;
            var rect = node.GetRect();
            
            await DragBy(new Vector2(rect.Size.x * delta.x, rect.Size.y * delta.y));
        }

        /// <summary>
        /// Drags the node by a multiple of its own size multiplied by the given factor.
        /// </summary>
        public async Task DragByOwnSize(float x, float y)
        {
            await DragByOwnSize(new Vector2(x, y));
        }


        /// <summary>
        /// Selects the given node by clicking on it.
        /// </summary>
        public async Task Select()
        {
            var node = PresentRoot;
            var rect = node.GetGlobalRect();

            // we assume that a position 5 pixels below the top border horizontally centered is a safe selection
            // spot as the node will have a title there.
            var selectionSpot = rect.Position + new Vector2(rect.Size.x / 2, 5);

            await Viewport.ClickMouseAt(selectionSpot);
            await node.WithinSeconds(3, () => Selected);
        }
        
        
        
        /// <summary>
        /// Drags a connection from the given source port of this node to the given target port of the given target node.
        /// </summary>
        public async Task DragConnection(Port sourcePort, GraphNodeDriver targetNode, Port targetPort)
        {
            if (!sourcePort.IsDefined)
            {
                throw new ArgumentException("Source port is not defined.");
            }

            if (!targetPort.IsDefined)
            {
                throw new ArgumentException("Target port is not defined.");
            }
            var thisRoot = PresentRoot;
            var targetRoot = targetNode.PresentRoot;

            if (sourcePort.IsInput && sourcePort.PortIndex >= thisRoot.GetConnectionInputCount())
            {
                throw new ArgumentException($"Node has no input port at the given index {sourcePort.PortIndex}.");
            }
            
            if (sourcePort.IsOutput && sourcePort.PortIndex >= thisRoot.GetConnectionOutputCount())
            {
                throw new ArgumentException($"Node has no output port at the given index {sourcePort.PortIndex}.");
            }
            
            if (targetPort.IsInput && targetPort.PortIndex >= targetRoot.GetConnectionInputCount())
            {
                throw new ArgumentException($"Target node has no input port at the given index {targetPort.PortIndex}.");
            }
            
            if (targetPort.IsOutput && targetPort.PortIndex >= targetRoot.GetConnectionOutputCount())
            {
                throw new ArgumentException($"Target node has no output port at the given index {targetPort.PortIndex}.");
            }

            var startPosition = sourcePort.IsInput 
                ? thisRoot.GetConnectionInputPosition(sourcePort.PortIndex) 
                : thisRoot.GetConnectionOutputPosition(sourcePort.PortIndex);
            var endPosition = targetPort.IsInput 
                ? targetRoot.GetConnectionInputPosition(targetPort.PortIndex) 
                : targetRoot.GetConnectionOutputPosition(targetPort.PortIndex);

            await Viewport.DragMouse(startPosition + thisRoot.RectGlobalPosition, endPosition + targetRoot.RectGlobalPosition);
        }
        
        /// <summary>
        /// Drags a connection from the given source port of this node to a position relative to this port.
        /// </summary>
        public async Task DragConnection(Port sourcePort, Vector2 relativePosition)
        {
            if (!sourcePort.IsDefined)
            {
                throw new ArgumentException("Source port is not defined.");
            }

            var thisRoot = PresentRoot;

            if (sourcePort.IsInput && sourcePort.PortIndex >= thisRoot.GetConnectionInputCount())
            {
                throw new ArgumentException($"Node has no input port at the given index {sourcePort.PortIndex}.");
            }
            
            if (sourcePort.IsOutput && sourcePort.PortIndex >= thisRoot.GetConnectionOutputCount())
            {
                throw new ArgumentException($"Node has no output port at the given index {sourcePort.PortIndex}.");
            }

            var startPosition = sourcePort.IsInput 
                ? thisRoot.GetConnectionInputPosition(sourcePort.PortIndex) 
                : thisRoot.GetConnectionOutputPosition(sourcePort.PortIndex);
            var endPosition = startPosition + relativePosition;

            await Viewport.DragMouse(startPosition + thisRoot.RectGlobalPosition, endPosition + thisRoot.RectGlobalPosition);
        }
    }
}