using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public abstract class LineEditBase<TLiteral> : LiteralWidgetBase<LineEdit, TLiteral> where TLiteral : IScadLiteral
    {
        protected abstract string LiteralValue { get; }

        protected override LineEdit CreateControl()
        {
            var lineEdit = Prefabs.New<SelectOnFocusLineEdit>();
            lineEdit.ExpandToTextLength = true;
            lineEdit.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            return lineEdit;
        }

        protected override void ApplyControlValue()
        {
            Control.Text = LiteralValue;
        }

        protected abstract void OnFocusExited();

    }
}