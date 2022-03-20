using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class Vector3Edit : VBoxContainer, IScadLiteralWidget
    {
        private NumberEdit _x;
        private NumberEdit _y;
        private NumberEdit _z;

        // this is a crude hack but i really don't feel like reimplementing the number edit here.
        private NumberLiteral _xLiteral; 
        private NumberLiteral _yLiteral; 
        private NumberLiteral _zLiteral;
        private Vector3Literal _vector3Literal;

        [Signal]
        public delegate void Changed();
        
        public ConnectExt.ConnectBinding ConnectChanged()
        {
            return this.Connect(nameof(Changed));
        }

        public void SetEnabled(bool enabled)
        {
            _x.SetEnabled(enabled);
            _y.SetEnabled(enabled);
            _z.SetEnabled(enabled);
        }

        public void NotifyChanged()
        {
            _vector3Literal.X = _xLiteral.Value;
            _vector3Literal.Y = _yLiteral.Value;
            _vector3Literal.Z = _zLiteral.Value;
            
            EmitSignal(nameof(Changed));
        }

        public void BindTo(Vector3Literal vector3Literal)
        {
            _vector3Literal = vector3Literal;
        } 
        
        public override void _Ready()
        {
            // we use some fake number literals to drive the number edits and then forward
            // the changes to the vector3 literal.
            _xLiteral = new NumberLiteral(0);
            _yLiteral = new NumberLiteral(0);
            _zLiteral = new NumberLiteral(0);

            _xLiteral.Value = _vector3Literal.X;
            _yLiteral.Value = _vector3Literal.Y;
            _zLiteral.Value = _vector3Literal.Z;
            
            _x = Prefabs.New<NumberEdit>();
            _x.BindTo(_xLiteral);
            _x.ConnectChanged().To(this, nameof(NotifyChanged));

            _y = Prefabs.New<NumberEdit>();
            _y.BindTo(_yLiteral);
            _y.ConnectChanged().To(this, nameof(NotifyChanged));
            
            _z = Prefabs.New<NumberEdit>();
            _z.BindTo(_zLiteral);
            _z.ConnectChanged().To(this, nameof(NotifyChanged));

            var xLabel = new Label();
            xLabel.Text = "X";
            xLabel.MoveToNewParent(this);
            _x.MoveToNewParent(this);


            var yLabel = new Label();
            yLabel.Text = "Y";
            yLabel.MoveToNewParent(this);
            _y.MoveToNewParent(this);

            var zLabel = new Label();
            zLabel.Text = "Z";
            zLabel.MoveToNewParent(this);
            _z.MoveToNewParent(this);
        }
    }
}