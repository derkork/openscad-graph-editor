using System;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public interface IScadLiteralWidget
    {

        void SetEnabled(bool enabled);
        
        event Action<object > LiteralValueChanged;
        event Action<bool> LiteralToggled;
    }
}