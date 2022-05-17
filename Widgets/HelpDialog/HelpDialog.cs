using System.Linq;
using Godot;
using GodotExt;
using JetBrains.Annotations;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets.HelpDialog
{
    [UsedImplicitly]
    public class HelpDialog : MarginContainer
    {
        private Label _titleLabel;
        private Label _descriptionLabel;
        private Container _leftContainer;
        private Container _rightContainer;
        private Container _nodeContainer;
        private ScadNodeWidget _widget;
        private Button _closeButton;

        public override void _Ready()
        {
        
            _titleLabel = this.WithName<Label>("TitleLabel");
            _descriptionLabel = this.WithName<Label>("DescriptionLabel");
            _leftContainer = this.WithName<Container>("LeftContainer");
            _rightContainer = this.WithName<Container>("RightContainer");
            _nodeContainer = this.WithName<Container>("NodeContainer");
            _closeButton = this.WithName<Button>("CloseButton");
            
            _closeButton.Connect("pressed")
                .To(this, nameof(OnCloseButtonPressed));
        }


        public void Open(IScadGraph graph, ScadNode node)
        {
            if (IsInstanceValid(_widget))
            {
                _widget.RemoveAndFree();
            }
            
            _widget = node is IHaveCustomWidget iUseCustomWidget
                ? iUseCustomWidget.InstantiateCustomWidget()
                : Prefabs.New<ScadNodeWidget>();

            _widget.MoveToNewParent(_nodeContainer);
            _widget.BindTo(graph, node);
            _widget.HintTooltip = "";

            _titleLabel.Text = node.NodeTitle.Trimmed(50);
            _descriptionLabel.Text = node.NodeDescription
                .OrDefault("<no documentation available>")
                .Trimmed(500);
            

            _leftContainer.GetChildNodes<Label>().ForAll(it => it.RemoveAndFree());
            _rightContainer.GetChildNodes<Label>().ForAll(it => it.RemoveAndFree());

            for (var i = 0; i < node.InputPortCount; i++)
            {
                var helpText = node.GetPortDocumentation(PortId.Input(i)).OrDefault("<no documentation available>").Trimmed(200);
                
                var label = new Label();
                label.Text = helpText;
                label.Autowrap = true;
                label.Align = Label.AlignEnum.Right;
                label.MoveToNewParent(_leftContainer);
            }

            for (var i = 0; i < node.OutputPortCount; i++)
            {
                var helpText = node.GetPortDocumentation(PortId.Output(i))
                    .OrDefault("<no documentation available>")
                    .Trimmed(200);
                
                var label = new Label();
                label.Text = helpText;
                label.Autowrap = true;
                label.Align = Label.AlignEnum.Left;
                label.MoveToNewParent(_rightContainer);
            }

            Visible = true;
            FocusMode = FocusModeEnum.All;
            GrabFocus();
            CallDeferred("update");
        }

        public override void _Draw()
        {
            if (!Visible)
            {
                return;
            }
            
            const float lineWidth = 2;
            DrawRect(GetRect(), new Color(0.1f,0.1f,0.1f,0.9f));
            var leftLabels = _leftContainer.GetChildNodes<Control>().ToList();
            var rightLabels = _rightContainer.GetChildNodes<Control>().ToList();
                
            // walk over left labels, and draw a line from the right side of the label to the
            // corresponding input port of the node widget.
            for (var i = 0; i < leftLabels.Count; i++)
            {
                var label = leftLabels[i];
                var startingPoint = label.RectGlobalPosition + new Vector2(label.RectSize.x + 10, label.RectSize.y/2);
                var endingPoint = _widget.GetConnectionInputPosition(i) + _widget.RectGlobalPosition;
                DrawLine(startingPoint, endingPoint, Colors.White, lineWidth, true);
            }
            
            // same for right labels
            for (var i = 0; i < rightLabels.Count; i++)
            {
                var label = rightLabels[i];
                var startingPoint = label.RectGlobalPosition + new Vector2(-10, label.RectSize.y/2);
                var endingPoint = _widget.GetConnectionOutputPosition(i) + _widget.RectGlobalPosition;
                DrawLine(startingPoint, endingPoint, Colors.White, lineWidth, true);
            }
        }

        public override void _Notification(int what)
        {
            base._Notification(what);
            if (what == NotificationResized)
            {
                Update();
            }
        }

        public override void _GuiInput(InputEvent evt)
        {
            if (Visible && evt.IsEscape())
            {
                OnCloseButtonPressed();
            }
        }

        private void OnCloseButtonPressed()
        {
            Visible = false;
        }
    }
}
