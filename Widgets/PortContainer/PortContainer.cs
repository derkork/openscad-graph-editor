using Godot;
using GodotExt;
using JetBrains.Annotations;

namespace OpenScadGraphEditor.Widgets.PortContainer
{
    public class PortContainer : Container
    {
        private Container _innerContainer;
        private Label _label;
        private Control _widget;

        public override void _Ready()
        {
            _innerContainer = this.WithName<Container>("InnerContainer");
            _label = this.WithName<Label>("Label");
        }

        public void Setup(bool left, string text, [CanBeNull] Control widget = null)
        {
            if (_widget != null && _widget != widget && IsInstanceValid(_widget) && _widget.GetParent() == _innerContainer)
            {
                _innerContainer.RemoveChild(_widget);
            }
            
            _widget = widget;
            _label.Align = left ? Label.AlignEnum.Left : Label.AlignEnum.Right;
            _label.Text = text;
            widget?.MoveToNewParent(_innerContainer);
        }

        public void Clear()
        {
            Visible = false;
        }

    }
}
