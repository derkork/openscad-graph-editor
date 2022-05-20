using Godot;
using GodotExt;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.NotificationBubble
{
    public class NotificationBubble : Node
    {

        [Export]
        public Color Color;
        [Export]
        public Color ErrorColor;
        
        private string _text;
        
        public override async void _Ready()
        {
            // set text
            this.WithName<Label>("Label").Text = _text;
            
            // play fade in animation
            var animationPlayer = this.WithName<AnimationPlayer>("AnimationPlayer");
            animationPlayer
                .Play("FadeIn");
            
            // wait for animation to finish
            await animationPlayer.FiresSignal("animation_finished");

            // 25 letters per second reading speed plus 1 second 
            await this.Sleep(1 + _text.Length / 25f);
            
            // play fade out animation
            animationPlayer.PlayBackwards("FadeIn");
            // wait for animation to finish
            await animationPlayer.FiresSignal("animation_finished");
            
            // and destroy self
            this.RemoveAndFree();
        }


        public static NotificationBubble Create(string text, bool isError)
        {
            var result = Prefabs.InstantiateFromScene<NotificationBubble>();
            result._text = text;

            var colorRect = result.WithName<ColorRect>("ColorRect");
            if (isError)
            {
                colorRect.Color = result.ErrorColor;
            }
            else
            {
                colorRect.Color = result.Color;
            }
            
            return result;
        }
    }
}
