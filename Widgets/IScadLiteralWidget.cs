using GodotExt;

namespace OpenScadGraphEditor.Widgets
{
    public interface IScadLiteralWidget
    {

        void SetEnabled(bool enabled);

        ConnectExt.ConnectBinding ConnectChanged();
    }
}