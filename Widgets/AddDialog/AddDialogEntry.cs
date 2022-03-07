using System;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets.AddDialog
{
    public class AddDialogEntry
    {
        private readonly Func<ScadNode> _builder;

        public AddDialogEntry(Func<ScadNode> builder)
        {
            _builder = builder;
            ExampleNode = builder();
        }

        public ScadNode ExampleNode { get; }
        public ScadNode CreateCopy() => _builder();
    }
}