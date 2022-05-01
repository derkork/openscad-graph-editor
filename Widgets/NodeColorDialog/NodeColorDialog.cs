using System;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.NodeColorDialog
{
    public class NodeColorDialog : WindowDialog
    {
   
        /// <summary>
        /// Called when a color was selected.
        /// </summary>
        public event Action<IScadGraph, ScadNode, Color> ColorSelected;
        
        /// <summary>
        /// Called when the color was removed.
        /// </summary>
        public event Action<IScadGraph, ScadNode> ColorCleared;

        private IScadGraph _contextGraph;
        private ScadNode _contextNode;

        public override void _Ready()
        {
            
            var buttonRow = this.WithName<Container>("ButtonRow");
            // attach to all buttons int he button row
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

        private void OnColorButtonPressed(ColorButton.ColorButton button)
        {
            ColorSelected?.Invoke(_contextGraph, _contextNode, button.Color);
            Hide();
        }

        public void Open(IScadGraph contextGraph, ScadNode contextNode)
        {
            _contextNode = contextNode;
            _contextGraph = contextGraph;
            PopupCenteredMinsize();
        }
        
        private void OnClearColor()
        {
            ColorCleared?.Invoke(_contextGraph, _contextNode);
            Hide();
        }

        private void OnCancel()
        {
            Hide();
        }
    }
}
