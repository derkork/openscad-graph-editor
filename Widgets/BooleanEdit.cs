using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public class BooleanEdit : CheckBox, IScadLiteralWidget
    {
        private BooleanLiteral _booleanLiteral;

        [Signal]
        public delegate void Changed();

        public void SetEnabled(bool enabled)
        {
            Disabled = !enabled;
        }

        public ConnectExt.ConnectBinding ConnectChanged()
        {
            return this.Connect(nameof(Changed));
        }

        public override void _Ready()
        {
            this.Connect("toggled")
                .To(this, nameof(NotifyChanged));
        }

        private void NotifyChanged([UsedImplicitly] bool value)
        {
            _booleanLiteral.Value = value;
            EmitSignal(nameof(Changed));   
        }

        public void BindTo(BooleanLiteral booleanLiteral)
        {
            _booleanLiteral = booleanLiteral;
        }
    }
}