using System;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public interface IScadLiteralWidget
    {
        event Action<IScadLiteral > LiteralValueChanged;
        event Action<bool> LiteralToggled;
    }
}