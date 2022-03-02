using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class StringEdit : LineEditBase
    {
        public override string RenderedValue => "\"" + Text.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}