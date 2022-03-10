using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class StringEdit : LineEditBase
    {
        private StringLiteral _stringLiteral;

        public override void _Ready()
        {
            this.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));

            Text = _stringLiteral.Value;
        }

        public void BindTo(StringLiteral stringLiteral)
        {
            _stringLiteral = stringLiteral;
        }

        private void OnFocusExited()
        {
            _stringLiteral.Value = Text;
            EmitSignal(nameof(Changed));
        }
    }
}