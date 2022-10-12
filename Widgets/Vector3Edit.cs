using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class Vector3Edit : LiteralWidgetBase<GridContainer, Vector3Literal>
    {
        private LineEdit _x;
        private LineEdit _y;
        private LineEdit _z;

        protected override GridContainer CreateControl()
        {
            var root = new GridContainer();
            root.Columns = 3;
            
            // build X field
            _x = Prefabs.New<SelectOnFocusLineEdit>();
            _x.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _x.MoveToNewParent(root);

            
            // build Y field
            _y = Prefabs.New<SelectOnFocusLineEdit>();
            _y.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _y.MoveToNewParent(root);
            
            // build Z field
            _z = Prefabs.New<SelectOnFocusLineEdit>();
            _z.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            _z.MoveToNewParent(root);

            return root;
        }

        protected override void ApplyControlValue()
        {
            _x.Text = Literal.X.SafeToString();
            _y.Text = Literal.Y.SafeToString();
            _z.Text = Literal.Z.SafeToString();
        }

        private void OnFocusExited()
        {
            EmitValueChange(new Vector3Literal(ParseDouble(_x), ParseDouble(_y), ParseDouble(_z)));
        }
        
        private static double ParseDouble(LineEdit lineEdit)
        {
            if (lineEdit.Text.SafeTryParse(out var value))
            {
                return value;
            }

            return 0;
        }
    }
}