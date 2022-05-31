using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Library;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;
using Serilog;
using Color = Godot.Color;

namespace OpenScadGraphEditor.Widgets
{
    public class ScadNodeWidget : GraphNode
    {
        public event Action<PortId, object> LiteralValueChanged;
        public event Action<PortId, bool> LiteralToggled;

        /// <summary>
        /// Event raised when the size is changed. This is actually only supported by the comment node so
        /// all handling for this is moved down to the CommentWidget class.
        /// </summary>
        public event Action<Vector2> SizeChanged;


        private readonly Dictionary<PortId, IScadLiteralWidget> _literalWidgets =
            new Dictionary<PortId, IScadLiteralWidget>();

        protected virtual Theme UseTheme => Resources.StandardNodeWidgetTheme;

        public ScadNode BoundNode { get; protected set; }

        private Tween _tween;

        private Font _commentFont;

        public override void _Ready()
        {
            Theme = UseTheme;
            _commentFont = Theme.GetFont("title_font", "GraphNode");
            RectMinSize = new Vector2(32, 64);
        }


        public virtual void BindTo(ScadGraph graph, ScadNode node)
        {
            BoundNode = node;
            Title = node.NodeTitle;
            HintTooltip = node.NodeDescription;
            Offset = node.Offset;

            if (node is IAmAnExpression && !(node is IHaveMultipleExpressionOutputs))
            {
                HintTooltip = node.Render(graph, 0);
            }
            
            var modifiers = BoundNode.GetModifiers();
            if (modifiers.HasFlag(ScadNodeModifier.Disable))
            {
                Modulate = new Color(1, 1, 1, 0.3f);
            }
            else
            {
                Modulate = Colors.White;
            }

            var maxPorts = Mathf.Max(node.InputPortCount, node.OutputPortCount);

            var existingContainers = this.GetChildNodes<HBoxContainer>().ToList();

            var idx = 0;
            while (idx < maxPorts)
            {
                PortContainer.PortContainer left;
                PortContainer.PortContainer right;

                if (existingContainers.Count <= idx)
                {
                    // make a new container
                    var parent = new HBoxContainer();
                    parent.MoveToNewParent(this);
                    left = Prefabs.InstantiateFromScene<PortContainer.PortContainer>();
                    left.MoveToNewParent(parent);
                    right = Prefabs.InstantiateFromScene<PortContainer.PortContainer>();
                    right.MoveToNewParent(parent);
                }
                else
                {
                    left = existingContainers[idx].GetChild<PortContainer.PortContainer>(0);
                    right = existingContainers[idx].GetChild<PortContainer.PortContainer>(1);
                }

                if (node.InputPortCount > idx)
                {
                    BuildPort(left, graph, node, PortId.Input(idx));
                }
                else
                {
                    left.Clear();
                    SetSlotEnabledLeft(idx, false);
                }

                if (node.OutputPortCount > idx)
                {
                    BuildPort(right, graph, node, PortId.Output(idx));
                }
                else
                {
                    right.Clear();
                    SetSlotEnabledRight(idx, false);
                }

                idx++;
            }

            // remove any remaining containers
            for (var i = idx; i < existingContainers.Count; i++)
            {
                SetSlotEnabledLeft(i, false);
                SetSlotEnabledRight(i, false);
                // this will kill the widgets as well
                existingContainers[i].RemoveAndFree();

                // remove the references to the widgets (if any)
                _literalWidgets.Remove(PortId.Input(i));
                _literalWidgets.Remove(PortId.Output(i));
            }

            // set to minimum size. Needs to be called
            // deferred as it doesn't seem to have any effect when being called in the same
            // frame as when the widget is created.
            CallDeferred(nameof(Minimize));
        }

        public async void Flash()
        {
            if (_tween != null)
            {
                return;
            }

            _tween = new Tween();
            _tween.MoveToNewParent(this);

            var initialModulate = Modulate;

            _tween.InterpolateProperty(this, "modulate", Modulate, Colors.Yellow, 0.5f);
            _tween.Start();

            await _tween.AreAllCompleted();

            _tween.InterpolateProperty(this, "modulate", Modulate, initialModulate, 0.5f);
            _tween.Start();

            await _tween.AreAllCompleted();

            _tween.RemoveAndFree();
            _tween = null;
        }

        private void Minimize()
        {
            SetSize(new Vector2(1, 1));
            QueueSort();
        }

        private void BuildPort(PortContainer.PortContainer container, ScadGraph graph, ScadNode node, PortId port)
        {
            var portDefinition = node.GetPortDefinition(port);
            var idx = port.Port;

            var connectorPortType = portDefinition.PortType;
            if (port.IsInput)
            {
                SetSlotEnabledLeft(idx, true);
                SetSlotColorLeft(idx, ColorFor(connectorPortType));
                SetSlotTypeLeft(idx, (int) connectorPortType);
            }
            else
            {
                SetSlotEnabledRight(idx, true);
                SetSlotColorRight(idx, ColorFor(connectorPortType));
                SetSlotTypeRight(idx, (int) connectorPortType);
            }


            var isConnected = graph.IsPortConnected(node, port);


            IScadLiteralWidget literalWidget = null;

            _literalWidgets.TryGetValue(port, out var existingWidget);

            if (node.TryGetLiteral(port, out var literal))
            {
                switch (literal)
                {
                    case BooleanLiteral booleanLiteral:
                        if (!(existingWidget is BooleanEdit booleanEdit))
                        {
                            booleanEdit = Prefabs.New<BooleanEdit>();
                        }

                        booleanEdit.BindTo(booleanLiteral, port.IsOutput, portDefinition.LiteralIsAutoSet, isConnected);
                        literalWidget = booleanEdit;
                        break;

                    case NumberLiteral numberLiteral:
                        if (!(existingWidget is NumberEdit numberEdit))
                        {
                            numberEdit = Prefabs.New<NumberEdit>();
                        }

                        numberEdit.BindTo(numberLiteral, port.IsOutput, portDefinition.LiteralIsAutoSet, isConnected);
                        literalWidget = numberEdit;
                        break;

                    case StringLiteral stringLiteral:
                        if (!(existingWidget is StringEdit stringEdit))
                        {
                            stringEdit = Prefabs.New<StringEdit>();
                        }

                        stringEdit.BindTo(stringLiteral, port.IsOutput, portDefinition.LiteralIsAutoSet, isConnected);
                        literalWidget = stringEdit;
                        break;
                    
                    case NameLiteral nameLiteral:
                        if (!(existingWidget is NameEdit nameEdit))
                        {
                            nameEdit = Prefabs.New<NameEdit>();
                        }
                        nameEdit.BindTo(nameLiteral, port.IsOutput, portDefinition.LiteralIsAutoSet, isConnected);
                        literalWidget = nameEdit;
                        break;

                    case Vector3Literal vector3Literal:
                        if (!(existingWidget is Vector3Edit vector3Edit))
                        {
                            vector3Edit = Prefabs.New<Vector3Edit>();
                        }

                        vector3Edit.BindTo(vector3Literal, port.IsOutput, portDefinition.LiteralIsAutoSet, isConnected);
                        literalWidget = vector3Edit;
                        break;

                    case Vector2Literal vector2Literal:
                        if (!(existingWidget is Vector2Edit vector2Edit))
                        {
                            vector2Edit = Prefabs.New<Vector2Edit>();
                        }

                        vector2Edit.BindTo(vector2Literal, port.IsOutput, portDefinition.LiteralIsAutoSet, isConnected);
                        literalWidget = vector2Edit;
                        break;
                }
            }

            if (existingWidget != null && existingWidget != literalWidget)
            {
                Log.Debug("Widget type changed");
                // we replaced the widget with something else, so delete the existing widget
                ((Node) existingWidget).RemoveAndFree();
                existingWidget = null;
                _literalWidgets.Remove(port);
            }

            if (literalWidget != null)
            {
                // only wire the events if we have an new widget.
                if (existingWidget != literalWidget)
                {
                    literalWidget.LiteralToggled += (value) => LiteralToggled?.Invoke(port, value);
                    literalWidget.LiteralValueChanged += (value) => LiteralValueChanged?.Invoke(port, value);
                }

                _literalWidgets[port] = literalWidget;
            }

            container.Setup(port.IsInput, portDefinition.Name, (Control) literalWidget);
        }


        protected static Color ColorFor(PortType portType)
        {
            switch (portType)
            {
                case PortType.Flow:
                    return new Color(1, 1, 1);
                case PortType.Boolean:
                    return new Color(1, 0.4f, 0.4f);
                case PortType.Number:
                    return new Color(1, 0.8f, 0.4f);
                case PortType.Vector3:
                    return new Color(.8f, 0.8f, 1f);
                case PortType.Vector2:
                    return new Color(.9f, 0.9f, 1f);
                case PortType.Array:
                    return new Color(.5f, 0.5f, 1f);
                case PortType.String:
                    return new Color(1f, 1f, 0f);
                case PortType.Any:
                    return new Color(1, 0f, 1f);
                case PortType.Reroute:
                    return new Color(0, 0.8f, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }

        public override void _Notification(int what)
        {
            if (what != NotificationDraw || BoundNode == null)
            {
                return;
            }

            var hasColor = false;
            // if we have a modifier color for this node, draw a boundary of that color
            if (BoundNode.TryGetColorModifier(out var color))
            {
                var size = RectSize;
                DrawRect(new Rect2(16, -35, size.x - 32, 35), color);
                hasColor = true;
            }

            var modifiers = BoundNode.GetModifiers();
            var hasModifiers = modifiers != ScadNodeModifier.None;

            var widthOffset = RectSize.x / 2;
            Texture icon = null;
            if (modifiers.HasFlag(ScadNodeModifier.Debug))
            {
                icon = Resources.DebugIcon;
            }

            if (modifiers.HasFlag(ScadNodeModifier.Root))
            {
                icon = Resources.RootIcon;
            }

            if (modifiers.HasFlag(ScadNodeModifier.Background))
            {
                icon = Resources.BackgroundIcon;
            }

            if (icon != null)
            {
                if (hasColor)
                {
                    // draw an underlay so we can see the icon no matter which color is currently used
                    DrawRect(new Rect2(widthOffset - 20, -35, 40, 35), new Color(0, 0, 0, 0.5f));
                }

                DrawTextureRect(icon, new Rect2(widthOffset - 16, -32, 32, 32), false);
            }

            // draw comment
            if (BoundNode.TryGetComment(out var comment))
            {
                // calculate how big the comment will be.
                var commentSize = _commentFont.GetStringSize(comment);
                var lineHeight = _commentFont.GetHeight();
                // calculate the comment position
                
                var commentPosition = new Vector2(widthOffset - commentSize.x / 2,
                    // if the node has color or modifier, offset the comment above it
                    -commentSize.y - (hasColor || hasModifiers ? 35 : 0) - 8);
                
                var padding = new Vector2(10, 16);
                // draw an underlay so we can better read the text
                DrawRect(new Rect2(commentPosition.x - padding.x/2, commentPosition.y , commentSize.x + padding.x, commentSize.y + padding.y/2), new Color(0, .0f, .0f, 0.5f));
                
                // and finally draw the comment
                DrawString(_commentFont,  commentPosition + new Vector2(0, lineHeight), comment, new Color(1,1,1, 0.8f));
            }
        }

        protected void RaiseSizeChanged(Vector2 newSize)
        {
            SizeChanged?.Invoke(newSize);
        }
    }
}