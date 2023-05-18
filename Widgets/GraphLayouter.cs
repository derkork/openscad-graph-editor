using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Nodes.Reroute;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// This class contains a variety of functions to layout nodes in a graph.
    /// </summary>
    public class GraphLayout
    {
        private readonly ScadGraph _graph;
        private readonly IEditorContext _context;
        private readonly Func<Vector2, Vector2> _globalPositionResolver;

        public GraphLayout(ScadGraph graph, IEditorContext context, Func<Vector2,Vector2> globalPositionResolver)
        {
            _graph = graph;
            _context = context;
            _globalPositionResolver = globalPositionResolver;
        }

        public void AlignNodesLeft(List<ScadNodeWidget> widgets)
        {
            AlignNodes("Align left", widgets, true, false);
        }

        public void AlignNodesRight(List<ScadNodeWidget> widgets)
        {
           AlignNodes("Align right", widgets, false, false);
        }

        public void AlignNodesTop(List<ScadNodeWidget> widgets)
        {
            AlignNodes("Align top", widgets, true, true);
        }


        public void AlignNodesBottom(List<ScadNodeWidget> widgets)
        {
            AlignNodes("Align bottom", widgets, false, true);
        }

        private void AlignNodes(string title, List<ScadNodeWidget> widgets, bool smallestFirst, bool vertical)
        {
            if (!(widgets.Count > 1))
            {
                _context.ShowInfo("Please select at least two nodes to align them.");
                return;
            }

            Func<ScadNodeWidget, float> borderSelector = smallestFirst switch
            {
                true when vertical => w => w.TopBorder(),
                true => w => w.LeftBorder(),
                false when vertical => w => w.BottomBorder(),
                false => w => w.RightBorder()
            };

            var border = borderSelector(smallestFirst ? widgets.MinBy(borderSelector) : widgets.MaxBy(borderSelector));
            var refactorings = widgets
                .Select(it =>
                    it.MoveBy(_graph, vertical
                        ? new Vector2(0, border - borderSelector(it))
                        : new Vector2(border - borderSelector(it), 0))
                )
                .ToList();
            _context.PerformRefactorings(title, refactorings);
        }
        
         public void StraightenConnections(List<ScadNodeWidget> selectedWidgets)
        {
            // all selected nodes 
            var selectedNodes = selectedWidgets.Select(it => it.BoundNode).ToHashSet();
            // a dictionary for quick lookup of a widget by the id of its node
            var widgets = selectedWidgets.ToDictionary(it => it.BoundNode.Id, it => it);

            // relevant connections, these are all connections between the selected nodes
            var relevantConnections = _graph.GetAllConnections()
                .Where(it => selectedNodes.Contains(it.From) && selectedNodes.Contains(it.To))
                // also exclude connections which have a wireless node as target, as we don't layout those
                .Where(it => !(it.To.IsWirelessReroute()))
                .ToList();

            // first up remove all nodes which do not have any connections to any other selected node, there is nothing to
            // straighten for them.
            selectedNodes.RemoveWhere(it => !relevantConnections.Any(connection => connection.InvolvesNode(it)));

            // now we have two sets, one is the set of nodes that are starting points for straightening, the other
            // is the set of nodes we have not yet laid out.
            var nodesToStraighten = selectedNodes.ToHashSet();
            var startingPoints = new HashSet<ScadNode>();

            // We are going to move the nodes around but only do so after the 
            // straightening refactoring is run. However some calculations depend on knowing where
            // the node will be after the refactoring. So we make ourselves a little lookup table where
            // we have the current node positions by id, which we overwrite during the layout process
            var nodePositions = new Dictionary<string, Vector2>();
            nodesToStraighten.ForAll(it => nodePositions[it.Id] = it.Offset);

            var refactorings = new List<Refactoring>();

            // now while we still have nodes in the set
            while (nodesToStraighten.Count > 0)
            {
                // first check if we have any starting points, if not, take the rightmost node we still have to 
                // straighten and use that one as a starting point.
                if (!startingPoints.Any())
                {
                    var newStartingPoint = nodesToStraighten.OrderByDescending(it => it.Offset.x).First();
                    startingPoints.Add(newStartingPoint);
                    nodesToStraighten.Remove(newStartingPoint);
                }

                GdAssert.That(startingPoints.Any(), "Should have at least one starting point");

                // now pop a node from the starting points and go from there
                var currentNode = startingPoints.First();
                startingPoints.Remove(currentNode);

                if (currentNode.IsWirelessReroute())
                {
                    continue; // a wireless node has no incoming connections that could be straightened
                }

                var currentNodeConnections = relevantConnections
                    .Where(it => it.To == currentNode)
                    .OrderBy(it => it.ToPort);

                // now walk over the connections and see if we can straighten them
                foreach (var connection in currentNodeConnections)
                {
                    if (!nodesToStraighten.Contains(connection.From))
                    {
                        // the connection is to a node we already moved, so go to the next one.
                        continue;
                    }

                    // good, we can straighten this connection
                    // to do this, we need to get the port offset of the right side of the connection
                    // and then move the left side of the connection to that offset
                    var rightSideWidget = widgets[connection.To.Id];
                    var leftSideWidget = widgets[connection.From.Id];

                    var rightOffset = nodePositions[connection.To.Id];
                    var rightConnectorPosition =
                        _globalPositionResolver(rightSideWidget.GetConnectionInputPosition(connection.ToPort)) +
                        rightOffset;
                    var leftOffset = nodePositions[connection.From.Id];
                    var leftConnectorPosition =
                        _globalPositionResolver(leftSideWidget.GetConnectionOutputPosition(connection.FromPort)) +
                        leftOffset;

                    // we only move the left node and we only change the y axis
                    var deltaY = rightConnectorPosition.y - leftConnectorPosition.y;

                    // only run a refactoring if we actually need to
                    if (Mathf.Abs(deltaY) > 0.001f)
                    {
                        var newLeftOffset = new Vector2(leftOffset.x, leftOffset.y + deltaY);
                        // write it down into our intermediate node position lookup table
                        nodePositions[connection.From.Id] = newLeftOffset;
                        refactorings.Add(new ChangeNodePositionRefactoring(_graph, connection.From, newLeftOffset));
                    }

                    // now we can remove the node from the set of nodes to straighten but it can still be a starting
                    // point for other nodes
                    nodesToStraighten.Remove(connection.From);
                    startingPoints.Add(connection.From);
                }
            }

            if (refactorings.Any())
            {
                _context.PerformRefactorings("Straighten connections", refactorings);
            }
            else
            {
                NotificationService.ShowNotification("Everything is already straightened");
            }
        }
    }

    /// <summary>
    /// Useful extension functions used when calculation graph layouts.
    /// </summary>
    internal static class GraphLayoutExt
    {
        /// <summary>
        /// Returns the right border of the given widget.
        /// </summary>
        internal static float RightBorder(this ScadNodeWidget widget)
        {
            return widget.BoundNode.Offset.x + widget.RectSize.x;
        }

        /// <summary>
        /// Returns the left border of the given widget.
        /// </summary>
        internal static float LeftBorder(this ScadNodeWidget widget)
        {
            return widget.BoundNode.Offset.x;
        }

        /// <summary>
        /// Returns the top border of the given widget.
        /// </summary>
        internal static float TopBorder(this ScadNodeWidget widget)
        {
            return widget.BoundNode.Offset.y;
        }

        /// <summary>
        /// Returns the bottom border of the given widget.
        /// </summary>
        internal static float BottomBorder(this ScadNodeWidget widget)
        {
            return widget.BoundNode.Offset.y + widget.RectSize.y;
        }

        /// <summary>
        /// Creates a refactoring that moves the given widget by the given offset.
        /// </summary>
        internal static ChangeNodePositionRefactoring MoveBy(this ScadNodeWidget widget, ScadGraph graph,
            Vector2 offset)
        {
            return new ChangeNodePositionRefactoring(graph, widget.BoundNode, widget.BoundNode.Offset + offset);
        }
    }
}