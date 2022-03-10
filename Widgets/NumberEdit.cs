using System.Globalization;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class NumberEdit : LineEditBase
    {
        private NumberLiteral _numberLiteral;

        public override void _Ready()
        {
            base._Ready();
            
            this.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            
            Text = _numberLiteral.Value.ToString(CultureInfo.InvariantCulture);
        }

        public void BindTo(NumberLiteral numberLiteral)
        {
            _numberLiteral = numberLiteral;
        }
        
        private void OnFocusExited()
        {
            if (!double.TryParse(Text, out var result))
            {
                Text = "0";
                _numberLiteral.Value = 0;
            }
            else
            {
                Text = result.ToString(CultureInfo.InvariantCulture);
                _numberLiteral.Value = result;
            }

            EmitSignal(nameof(Changed));
            DeselectAll();
        }
    }
}