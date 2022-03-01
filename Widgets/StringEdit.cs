using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class StringEdit : LineEditBase
    {
        public override string Value
        {
            get => Text.Replace("\\", "\\\\").Replace("\"", "\\\"");
            set => Text = value.Replace("\\\\", "\\").Replace("\\\"", "\"");
        }
    }
}