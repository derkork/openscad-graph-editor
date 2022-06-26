using System;
using System.Collections.Generic;
using Godot;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    /// <summary>
    /// A dialog entry which represents a pair of bound nodes.
    /// </summary>
    public class BoundPairBasedEntry : IAddDialogEntry
    {
        private readonly ICanPerformRefactorings _canPerformRefactorings;
        private readonly Func<ScadNode> _firstNodeFactory;
        private readonly Func<ScadNode> _secondNodeFactory;
        private readonly ScadNode _firstNodeTemplate;
        private readonly ScadNode _secondNodeTemplate;

        public string Title { get; }
        public string Keywords { get; }
        public Action<RequestContext> Action => OnEntrySelected;

        public Texture Icon { get; }

        public BoundPairBasedEntry(Texture icon, string title, string keywords,
            Func<ScadNode> firstNodeFactory, Func<ScadNode> secondNodeFactory,
            ICanPerformRefactorings canPerformRefactorings)
        {
            _firstNodeFactory = firstNodeFactory;
            _secondNodeFactory = secondNodeFactory;
            _canPerformRefactorings = canPerformRefactorings;
            _firstNodeTemplate = firstNodeFactory();
            _secondNodeTemplate = secondNodeFactory();
            Icon = icon;
            Title = title;
            Keywords = keywords;
        }

        public EntryFittingDecision CanAdd(RequestContext context)
        {
            if (!context.Source.Description.CanUse(_firstNodeTemplate) || !context.Source.Description.CanUse(_secondNodeTemplate))
            {
                // if any of the nodes is not allowed to be used here, we can't use it
                return EntryFittingDecision.Veto;
            }

            if (context.TryGetNodeAndPort(out var contextNode, out var contextPort))
            {
                // if this came from a node left of us, check if we have a matching input port
                if (contextPort.IsOutput)
                {
                    if (ConnectionRules.TryGetPossibleConnection(context.Source, contextNode, _firstNodeTemplate, contextPort,
                            out _))
                    {
                        return EntryFittingDecision.Fits;
                    }
                }
                // if this came from a node right of us, check if we have a matching output port
                else
                {
                    if (ConnectionRules.TryGetPossibleConnection(context.Source, _secondNodeTemplate, contextNode, contextPort,
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
            // make the new nodes
            var firstNode = _firstNodeFactory();
            var secondNode = _secondNodeFactory();
            
            // offset the second node from the first node so they don't overlap
            firstNode.Offset = context.Position;
            secondNode.Offset = context.Position + new Vector2(400, 0);
            
            // interconnect the nodes.
            ((IAmBoundToOtherNode)firstNode).OtherNodeId = secondNode.Id;
            ((IAmBoundToOtherNode)secondNode).OtherNodeId = firstNode.Id;
            
            
            context.TryGetNodeAndPort(out var otherNode, out var otherPort);

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
                new AddNodeRefactoring(context.Source, firstNode, incomingNode, otherPort),
                new AddNodeRefactoring(context.Source, secondNode, outgoingNode, otherPort)
            };
            
            // and run it.
            _canPerformRefactorings.PerformRefactorings("Add node", refactorings);
        }
    }
}