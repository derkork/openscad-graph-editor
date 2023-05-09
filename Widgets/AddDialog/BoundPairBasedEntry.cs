using System;
using System.Collections.Generic;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    /// <summary>
    /// A dialog entry which represents a pair of bound nodes.
    /// </summary>
    public class BoundPairBasedEntry : IAddDialogEntry
    {
        private readonly Func<ScadNode> _firstNodeFactory;
        private readonly Func<ScadNode> _secondNodeFactory;
        private readonly IEditorContext _editorContext;
        private readonly ScadNode _firstNodeTemplate;
        private readonly ScadNode _secondNodeTemplate;

        public string Title { get; }
        public string Keywords { get; }
        public Action<RequestContext> Action => OnEntrySelected;

        public Texture Icon { get; }

        public BoundPairBasedEntry(Texture icon, string title, string keywords,
            Func<ScadNode> firstNodeFactory, Func<ScadNode> secondNodeFactory,
            IEditorContext editorContext)
        {
            _firstNodeFactory = firstNodeFactory;
            _secondNodeFactory = secondNodeFactory;
            _editorContext = editorContext;
            _firstNodeTemplate = firstNodeFactory();
            _secondNodeTemplate = secondNodeFactory();
            Icon = icon;
            Title = title;
            Keywords = keywords;
        }

        public EntryFittingDecision CanAdd(RequestContext context)
        {
  

            if (context.TryGetNodeAndPort(out var graph, out var contextNode, out var contextPort))
            {
                if (!graph.Description.CanUse(_firstNodeTemplate) || !graph.Description.CanUse(_secondNodeTemplate))
                {
                    // if any of the nodes is not allowed to be used here, we can't use it
                    return EntryFittingDecision.Veto;
                }
                
                // if this came from a node left of us, check if we have a matching input port
                if (contextPort.IsOutput)
                {
                    if (ConnectionRules.TryGetPossibleConnection(graph, contextNode, _firstNodeTemplate, contextPort,
                            out _))
                    {
                        return EntryFittingDecision.Fits;
                    }
                }
                // if this came from a node right of us, check if we have a matching output port
                else
                {
                    if (ConnectionRules.TryGetPossibleConnection(graph, _secondNodeTemplate, contextNode, contextPort,
                            out _))
                    {
                        return EntryFittingDecision.Fits;
                    }
                }
            }


            // otherwise it doesn't match, but could still be added.
            return EntryFittingDecision.DoesNotFit;
        }


        private void OnEntrySelected(RequestContext context)
        {
            var hasPosition = context.TryGetPosition(out var graph, out var position);
            GdAssert.That(hasPosition, "BoundPairBasedEntry can only be used in a context with a position.");
            
            // make the new nodes
            var firstNode = _firstNodeFactory();
            var secondNode = _secondNodeFactory();
            
            // try getting a context node if it is there.
            context.TryGetNodeAndPort(out _, out var otherNode, out var otherPort);

            
            // offset the second node from the first node so they don't overlap
            firstNode.Offset = position;
            secondNode.Offset = position + new Vector2(400, 0);
            
            // interconnect the nodes.
            ((IAmBoundToOtherNode)firstNode).OtherNodeId = secondNode.Id;
            ((IAmBoundToOtherNode)secondNode).OtherNodeId = firstNode.Id;

            // if we have another node in the context, add a connection context node -> first node
            // if the context node is left of us, or add a connection second node -> context node if the
            // context node is right of us.
            ScadNode incomingNode = null;
            ScadNode outgoingNode = null;

            if (otherPort.IsInput)
            {
                incomingNode = otherNode;
            }

            if (otherPort.IsOutput)
            {
                outgoingNode = otherNode;
            }
            
            // build the list of refactorings
            var refactorings = new List<Refactoring>
            {
                new AddNodeRefactoring(graph, firstNode, incomingNode, otherPort),
                new AddNodeRefactoring(graph, secondNode, outgoingNode, otherPort)
            };
            
            // and run it.
            _editorContext.PerformRefactorings("Add node", refactorings);
        }
    }
}