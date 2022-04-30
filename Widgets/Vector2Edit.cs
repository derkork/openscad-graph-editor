using System.Globalization;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class Vector2Edit : LiteralWidgetBase<GridContainer, Vector2Literal>
    {
        private LineEdit _x;
        private LineEdit _y;

        protected override GridContainer CreateControl()
        {
            var root = new GridContainer();
            root.Columns = 2;
            
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
            

            return root;
        }

        protected override void ApplyControlValue()
        {
            _x.Text = Literal.X.ToString(CultureInfo.InvariantCulture);
            _y.Text = Literal.Y.ToString(CultureInfo.InvariantCulture);
        }

        private void OnFocusExited()
        {
            var array = new[]
            {
                ParseDouble(_x),
                ParseDouble(_y),
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