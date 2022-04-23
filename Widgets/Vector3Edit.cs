using System.Globalization;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class Vector3Edit : LiteralWidgetBase<VBoxContainer, Vector3Literal>
    {
        private LineEdit _x;
        private LineEdit _y;
        private LineEdit _z;
        
        protected override void DoSetEnabled(bool enabled)
        {
            _x.Editable = enabled;
            _y.Editable = enabled;
            _z.Editable = enabled;
        }

        protected override VBoxContainer CreateControl()
        {
            var root = new VBoxContainer();
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
            _x.Text = Literal.X.ToString(CultureInfo.InvariantCulture);
            _y.Text = Literal.Y.ToString(CultureInfo.InvariantCulture);
            _z.Text = Literal.Z.ToString(CultureInfo.InvariantCulture);
        }

        private void OnFocusExited()
        {
            var array = new[]
            {
                ParseDouble(_x),
                ParseDouble(_y),
                ParseDouble(_z)
            };
            
            EmitValueChange(array);
        }
        
        private static double ParseDouble(LineEdit lineEdit)
        {
            if (double.TryParse(lineEdit.Text, out var value))
            {
                return value;
            }

            return 0;
        }
    }
}