using System;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Nodes.ForLoop
{
    public class ForLoopAddDialogEntry : IAddDialogEntry
    {
        public string Title => "For loop";
        public string Keywords => "Builds a for loop.";
        public Action<RequestContext> Action { get; set; }
        public Texture Icon => Resources.ScadBuiltinIcon;

        private readonly ForLoopStart _forLoopStart = NodeFactory.Build<ForLoopStart>();

        
        public EntryFittingDecision CanAdd(RequestContext context)
        {
            if (!context.Source.Description.CanUse(_forLoopStart))
            {
                // if the node is not allowed to be used here, we can't use it
                return EntryFittingDecision.Veto;
            }

            return EntryFittingDecision.Fits;
        }
    }
}