using GodotExt;

namespace OpenScadGraphEditor.Widgets
{
    public interface IScadLiteralWidget
    {

        void SetEnabled(bool enabled);
        
        string RenderedValue { get; }
        
        string SerializedValue { get; set; }

        ConnectExt.ConnectBinding ConnectChanged();
    }
}