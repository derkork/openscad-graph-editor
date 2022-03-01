using Godot;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets
{
    [UsedImplicitly]
    public class NumberEdit : LineEditBase
    {
        public override void _Ready()
        {
            base._Ready();
            
            this.Connect("focus_exited")
                .To(this, nameof(OnFocusExited));
            Text = "0";
        }

        public override string Value
        {
            get => Text;
            set => Text = value;
        }

        private void OnFocusExited()
        {
            if (!double.TryParse(Text, out _))
            {
                Text = "0";
            }   
        }
    }
}