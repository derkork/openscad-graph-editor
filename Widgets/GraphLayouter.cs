using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
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

        public GraphLayout(ScadGraph graph, IEditorContext context)
        {
            _graph = graph;
            _context = context;
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