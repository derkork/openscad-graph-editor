using System;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public class DragData
    {
        public DragData(string name, Func<ScadNode> data)
        {
            Name = name;
            Data = data;
        }

        public Func<ScadNode> Data { get; }

        public string Name { get; }
        
    }
}