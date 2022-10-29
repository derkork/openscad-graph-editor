using Godot;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class StringEdit : LineEditBase<StringLiteral>
    {
        private int _maxLength;
        protected override string LiteralValue => Literal.Value;

        public int MaxLength
        {
            get => _maxLength;
            set
            {
                _maxLength = value;
                if (Control != null)
                {
                    Control.MaxLength = value;
                }
            }
        }

        protected override LineEdit CreateControl()
        {
            var result = base.CreateControl();
            result.MaxLength = MaxLength;
            return result;
        }


        protected override void OnFocusExited()
        {
            EmitValueChange(new StringLiteral(Control.Text));
        }
    }
}