using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class StringEdit : LineEditBase<StringLiteral>
    {
        protected override string LiteralValue => Literal.Value;


        protected override void OnFocusExited()
        {
            EmitValueChange(Control.Text);
        }
    }
}