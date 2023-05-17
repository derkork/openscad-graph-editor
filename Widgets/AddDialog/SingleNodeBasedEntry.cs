using System;
using Godot;
using OpenScadGraphEditor.Actions;
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
        private readonly IEditorContext _editorContext;

        public string Title => _template.NodeTitle + (_template.NodeQuickLookup.Length > 0 ?  " [" + _template.NodeQuickLookup + "]" : "");
        public string Keywords => _template.NodeDescription;
        public Action<RequestContext> Action => OnEntrySelected;

        public Texture Icon { get; }

        public SingleNodeBasedEntry(Texture icon, Func<ScadNode> factory, IEditorContext editorContext)
        {
            _factory = factory;
            _editorContext = editorContext;
            _template = _factory();
            Icon = icon;
        }

        public EntryFittingDecision CanAdd(RequestContext context)
        {
            if (context.TryGetNodeAndPort(out var graph, out var contextNode, out var contextPort))
            {
                // if the node is not allowed to be used here, we can't use it
                if (!graph.Description.CanUse(_template))
                {
                    return EntryFittingDecision.Veto;
                }
                
                // if this came from a node left of us, check if we have a matching input port
                if (contextPort.IsOutput)
                {
                    if (ConnectionRules.TryGetPossibleConnection(graph, contextNode, _template, contextPort,
                            out _))
                    {
                        return EntryFittingDecision.Fits;
                    }
                }
                // if this came from a node right of us, check if we have a matching output port
                else
                {
                    if (ConnectionRules.TryGetPossibleConnection(graph, _template, contextNode, contextPort,
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
            // get graph and position
            context.TryGetPositionInGraph(out var graph, out var position);
            // if we have, get the other node and port (note this returns null for everything if we have no node, so
            // we don't take graph and position from this call
            context.TryGetNodeAndPort(out _, out var otherNode, out var otherPort);
            node.Offset = position;

            _editorContext.PerformRefactoring("Add node",
                new AddNodeRefactoring(graph, node, otherNode, otherPort));
        }
    }
}