using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Widgets
{
    /// <summary>
    /// Super simple notification service to show a notification from everywhere. Not sure if i like this "design"
    /// but it is the simplest solution that works.
    /// </summary>
    public class NotificationService : Node
    {
        private static NotificationService _instance;

        public override void _Ready()
        {
            _instance = this;
        }
        
        public static void ShowNotification(string message)
        {
            var bubble = NotificationBubble.NotificationBubble.Create(message);
            bubble.MoveToNewParent(_instance);
        }
    }
}