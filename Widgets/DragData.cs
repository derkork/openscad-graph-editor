using System;

namespace OpenScadGraphEditor.Widgets
{
    public readonly struct DragData
    {
        public DragData(string name, object data)
        {
            Name = name;
            Data = data;
        }

        public object Data { get; }

        public string Name { get; }
        
    }
}