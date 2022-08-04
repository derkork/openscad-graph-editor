using System.Globalization;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class NumberEdit : LineEditBase<NumberLiteral>
    {
        protected override string LiteralValue => Literal.Value.SafeToString();


        protected override void OnFocusExited()
        {
            EmitValueChange(!Control.Text.SafeTryParse(out var result) ? 0d : result);
        }
    }
}