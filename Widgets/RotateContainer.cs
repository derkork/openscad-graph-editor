using System.Linq;
using Godot;
using GodotExt;

namespace OpenScadGraphEditor.Widgets
{
    [Tool]
    public class RotateContainer : Container
    {
        private RotateMode _mode;

        public enum RotateMode
        {
            Left,
            Right
        }

        [Export]
        public RotateMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                EmitSignal(nameof(MinimumSizeChanged).ToSnakeCase());
                QueueSort();
            }
        }

        public override Vector2 _GetMinimumSize()
        {
            var minSize = new Vector2();
            minSize = this.GetChildNodes<Control>()
                .Select(child => child.GetCombinedMinimumSize())
                .Aggregate(minSize,
                    (current, childMinSize) => new Vector2(Mathf.Max(current.x, childMinSize.x),
                        Mathf.Max(current.y, childMinSize.y)));

            return new Vector2(minSize.y, minSize.x);
        }

        public override void _Notification(int what)
        {
            switch (what)
            {
                case NotificationSortChildren:
                {
                    var size = this.RectSize;
                    var flippedSize = new Vector2(size.y, size.x);
                    foreach (var child in this.GetChildNodes<Control>())
                    {
                        FitChildInRect(child, new Rect2(Vector2.Zero, flippedSize));
                        switch (Mode)
                        {
                            case RotateMode.Left:
                                child.RectPosition -= new Vector2(child.RectSize.x, 0);
                                child.RectPivotOffset = new Vector2(child.RectSize.x, 0);
                                child.RectRotation = -90;
                                break;
                            case RotateMode.Right:
                                child.RectPosition += new Vector2(child.RectSize.y, 0);
                                child.RectPivotOffset = Vector2.Zero;
                                child.RectRotation = 90;
                                break;
                        }
                    }

                    break;
                }
                case NotificationThemeChanged:
                    EmitSignal(nameof(MinimumSizeChanged).ToSnakeCase());
                    break;
            }
        }
    }
}