using GodotExt;

namespace OpenScadGraphEditor.Widgets
{
    public interface IScadLiteralWidget
    {

        void SetEnabled(bool enabled);
        
        string Value { get; set; }

        ConnectExt.ConnectBinding ConnectChanged();
    }
}