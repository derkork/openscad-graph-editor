using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Refactorings
{
    /// <summary>
    /// Pastes all nodes from the source buffer into the target graph at the given position. If pasteCopy is true,
    /// the source buffer will be duplicated before pasting, so that the original source buffer is not modified.
    /// Only nodes that are allowed in the target graph will be pasted.
    /// Provides the pasted nodes as ticket data.
    /// </summary>
    public class PasteNodesRefactoring : Refactoring, IProvideTicketData<List<ScadNode>>
    {
        private readonly ScadGraph _targetGraph;
        private readonly ScadGraph _sourceBuffer;
        private readonly Vector2 _position;
        private readonly bool _pasteCopy;

        public string Ticket { get; } = Guid.NewGuid().ToString();

        public PasteNodesRefactoring(ScadGraph targetGraph, ScadGraph sourceBuffer, Vector2 position, bool pasteCopy = true)
        {
            _targetGraph = targetGraph;
            _sourceBuffer = sourceBuffer;
            _position = position;
            _pasteCopy = pasteCopy;
        }
        
        public override void PerformRefactoring(RefactoringContext context)
        {
            var copy = _pasteCopy 
	            ? _sourceBuffer.CloneSelection(context.Project, _sourceBuffer.GetAllNodes(), out _) 
	            : _sourceBuffer;
            
            // now the buffer may contain nodes that are not allowed in the given target graph. So we need
			// to filter these out here and also delete all connections to these nodes.
			var disallowedNodes = copy.GetAllNodes()
				.Where(it => !_targetGraph.Description.CanUse(it))
				.ToList();

			// delete all connections to the disallowed nodes
			copy.GetAllConnections()
				.Where(it => disallowedNodes.Any(it.InvolvesNode))
				.ToList() // avoid concurrent modification
				.ForAll(copy.RemoveConnection);

			// and the nodes themselves
			disallowedNodes.ForAll(copy.RemoveNode);

			var scadNodes = copy.GetAllNodes().ToList();
			if (scadNodes.Count == 0)
			{
				return;
			}

			// we now need to normalize the position of the nodes so they are pasted in the correct position
			// we do this by finding the bounding box of the nodes and then offsetting them by the difference between the position
			// of the bounding box and the position of the node that is closes to top left

			// we start with a rectangle that is a point that simply has the position of the first node
			var boundingBox = new Rect2(scadNodes[0].Offset, Vector2.Zero);
			// now expand this rectangle so it contains all the points of all the offsets of the nodes
			boundingBox = scadNodes.Aggregate(boundingBox, (current, node) => current.Expand(node.Offset));
			// now we calculate the offset of the bounding box position and the desired position
			var offset = _position - boundingBox.Position;

			// and offset every node by this
			scadNodes.ForAll(it => it.Offset += offset);

			// build the refactorings to add the given nodes and connections
			var refactorings = new List<Refactoring>();
			foreach (var node in scadNodes)
			{
				refactorings.Add(new AddNodeRefactoring(_targetGraph, node));
			}

			foreach (var connection in copy.GetAllConnections())
			{
				refactorings.Add(
					new AddConnectionRefactoring(
						new ScadConnection(_targetGraph, connection.From, connection.FromPort, connection.To,
							connection.ToPort
						)
					)
				);
			}
			
			// and finally perform all the refactorings
			refactorings.ForAll(context.PerformRefactoring);
			
			context.StoreTicketData(this, scadNodes);
        }
    }
}