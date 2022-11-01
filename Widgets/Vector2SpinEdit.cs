using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public class Vector2SpinEdit : SpinEditLiteralWidgetBase<GridContainer, Vector2Literal>
    {
        private SpinBox _x;
        private SpinBox _y;

        protected override GridContainer CreateControl()
        {
            var root = new GridContainer();
            root.Columns = 2;
            
            // build X field
            _x = new SpinBox();
            _x.GetLineEdit()
                .Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _x.SizeFlagsHorizontal = (int) SizeFlags.ExpandFill;
            _x.MoveToNewParent(root);
            
            // build Y field
            _y = new SpinBox();
            _y.GetLineEdit()
                .Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _y.SizeFlagsHorizontal = (int) SizeFlags.ExpandFill;
            _y.MoveToNewParent(root);
            
            
            
            SetupRange();

            return root;
        }

        protected override void ApplyControlValue()
        {   
            // single | is on purpose here so both values are always set
            if (ApplyValue(_x, Literal.X, out var newX) 
                | ApplyValue(_y, Literal.Y, out var newY))
            {
                EmitValueChange(new Vector2Literal(newX, newY));
            }

        }

        protected override void SetupRange()
        {
            ApplyRange(_x);
            ApplyRange(_y);
        }

        private void OnFocusExited()
        {
            EmitValueChange(new Vector2Literal(_x.Value, _y.Value));
        }
    }
}