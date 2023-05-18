using System;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Actions;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Refactorings;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.NodeColorDialog
{
    [UsedImplicitly]
    public class NodeColorDialog : WindowDialog
    {
        private ScadGraph _graph;
        private ScadNode _node;
        private IEditorContext _context;

        public override void _Ready()
        {
            var buttonRow = this.WithName<Container>("ButtonRow");
            // attach to all buttons in the button row
            buttonRow.GetChildNodes<ColorButton.ColorButton>()
                .ForAll(it => it.Pressed += OnColorButtonPressed);
            
            // add a button to clear the color
            this.WithName<Button>("ClearButton")
                .Connect("pressed")
                .To(this, nameof(OnClearColor));
            
            // connect the cancel button
            this.WithName<Button>("CancelButton")
                .Connect("pressed")
                .To(this, nameof(OnCancel));
        }
        
        public void Open(IEditorContext context, ScadGraph contextGraph, ScadNode contextNode)
        {
            _node = contextNode;
            _graph = contextGraph;
            _context = context;
            PopupCenteredMinsize();
        }

        private void OnColorButtonPressed(ColorButton.ColorButton button)
        {
            _context.PerformRefactoring("Change node color",
                new ToggleModifierRefactoring(_graph, _node, ScadNodeModifier.Color, true, button.Color));
            Hide();
        }

        private void OnClearColor()
        {
            _context.PerformRefactoring("Clear node color",
                new ToggleModifierRefactoring(_graph, _node, ScadNodeModifier.Color, false));
            Hide();
        }

        private void OnCancel()
        {
            Hide();
        }
    }
}
