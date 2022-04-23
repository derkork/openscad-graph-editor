using System.Globalization;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class NumberEdit : LineEditBase<NumberLiteral>
    {
        protected override string LiteralValue => Literal.Value.ToString(CultureInfo.InvariantCulture);


        protected override void OnFocusExited()
        {
            EmitValueChange(!double.TryParse(Control.Text, out var result) ? 0d : result);
        }
    }
}