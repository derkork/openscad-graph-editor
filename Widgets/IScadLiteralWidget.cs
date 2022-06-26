using System;

namespace OpenScadGraphEditor.Widgets
{
    public interface IScadLiteralWidget
    {
        event Action<object > LiteralValueChanged;
        event Action<bool> LiteralToggled;
    }
}