using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;

namespace OpenScadGraphEditor.Widgets
{
    public class Vector3SpinEdit : SpinEditLiteralWidgetBase<GridContainer, Vector3Literal>
    {
        private SpinBox _x;
        private SpinBox _y;
        private SpinBox _z;

        protected override GridContainer CreateControl()
        {
            var root = new GridContainer();
            root.Columns = 3;
            
            // build X field
            _x = new SpinBox();
            _x.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _x.SizeFlagsHorizontal = (int) SizeFlags.ExpandFill;
            _x.MoveToNewParent(root);
            
            // build Y field
            _y = new SpinBox();
            _y.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _y.SizeFlagsHorizontal = (int) SizeFlags.ExpandFill;
            _y.MoveToNewParent(root);
            
            // build Z field
            _z = new SpinBox();
            _z.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _z.SizeFlagsHorizontal = (int) SizeFlags.ExpandFill;
            _z.MoveToNewParent(root);

            SetupRange();

            return root;
        }

        protected override void ApplyControlValue()
        {   
            // single | is on purpose here so both values are always set
            if (ApplyValue(_x, Literal.X, out var newX) 
                | ApplyValue(_y, Literal.Y, out var newY)
                | ApplyValue(_z, Literal.Z, out var newZ))
            {
                EmitValueChange(new Vector3Literal(newX, newY, newZ));
            }

        }

        protected override void SetupRange()
        {
            ApplyRange(_x);
            ApplyRange(_y);
            ApplyRange(_z);
        }

        private void OnFocusExited()
        {
            EmitValueChange(new Vector3Literal(_x.Value, _y.Value, _z.Value));
        }
    }
}