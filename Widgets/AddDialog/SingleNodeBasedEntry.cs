using System;
using Godot;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    /// <summary>
    /// A dialog entry which will produce a single node.
    /// </summary>
    public class SingleNodeBasedEntry : IAddDialogEntry
    {
        private readonly ScadNode _template;
        private readonly Func<ScadNode> _factory;
        private readonly ICanPerformRefactorings _canPerformRefactorings;

        public string Title => _template.NodeTitle;
        public string Keywords => _template.NodeDescription;
        public Action<RequestContext> Action => OnEntrySelected;

        public Texture Icon { get; }

        public SingleNodeBasedEntry(Texture icon, Func<ScadNode> factory, ICanPerformRefactorings canPerformRefactorings)
        {
            _factory = factory;
            _canPerformRefactorings = canPerformRefactorings;
            _template = _factory();
            Icon = icon;
        }

        public EntryFittingDecision CanAdd(RequestContext context)
        {
            if (!context.Source.Description.CanUse(_template))
            {
                // if the node is not allowed to be used here, we can't use it
                return EntryFittingDecision.Veto;
            }

            if (context.TryGetNodeAndPort(out var contextNode, out var contextPort))
            {
                // if this came from a node left of us, check if we have a matching input port
                if (contextPort.IsOutput)
                {
                    if (ConnectionRules.TryGetPossibleConnection(context.Source, contextNode, _template, contextPort,
                            out _))
                    {
                        return EntryFittingDecision.Fits;
                    }
                }
                // if this came from a node right of us, check if we have a matching output port
                else
                {
                    if (ConnectionRules.TryGetPossibleConnection(context.Source, _template, contextNode, contextPort,
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
            // build a new node, put it to the correct position and then add it using the refactoring facility
            var node = _factory();
            node.Offset = context.Position;
            context.TryGetNodeAndPort(out var otherNode, out var otherPort);

            _canPerformRefactorings.PerformRefactorings("Add node",
                new AddNodeRefactoring(context.Source, node, otherNode, otherPort));
        }
    }
}