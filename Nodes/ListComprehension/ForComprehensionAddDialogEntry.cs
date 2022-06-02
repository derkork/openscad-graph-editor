using System;
using Godot;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes.ForLoop;
using OpenScadGraphEditor.Nodes.ListComprehension;
using OpenScadGraphEditor.Utils;
using OpenScadGraphEditor.Widgets.AddDialog;

namespace OpenScadGraphEditor.Nodes.ForComprehension
{
    public class ForComprehensionAddDialogEntry : IAddDialogEntry
    {
        public string Title => "For comprehension";
        public string Keywords => "Builds a for-comprehension.";
        public Action<RequestContext> Action { get; set; }
        public Texture Icon => Resources.ScadBuiltinIcon;

        private readonly ForComprehensionStart _forComprehensionStart = NodeFactory.Build<ForComprehensionStart>();

        
        public EntryFittingDecision CanAdd(RequestContext context)
        {
            if (!context.Source.Description.CanUse(_forComprehensionStart))
            {
                // if the node is not allowed to be used here, we can't use it
                return EntryFittingDecision.Veto;
            }

            return EntryFittingDecision.Fits;
        }
    }
}