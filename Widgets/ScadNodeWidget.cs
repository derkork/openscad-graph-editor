using System;
using System.Collections.Generic;
using Godot;
using GodotExt;
using OpenScadGraphEditor.Nodes;
using OpenScadGraphEditor.Utils;

namespace OpenScadGraphEditor.Widgets
{
    public class ScadNodeWidget : GraphNode
    {
        [Signal]
        public delegate void Changed();

        private readonly Dictionary<int, IScadLiteralWidget> _inputLiteralWidgets =
            new Dictionary<int, IScadLiteralWidget>();

        private readonly Dictionary<int, IScadLiteralWidget> _outputLiteralWidgets =
            new Dictionary<int, IScadLiteralWidget>();

        public override void _Ready()
        {
            this.Connect("offset_changed")
                .To(this, nameof(OnOffsetChanged));
        }

        private void OnOffsetChanged()
        {
            BoundNode.Offset = Offset;
            NotifyChanged();
        }
        
        public ScadNode BoundNode { get; private set; }

        public void BindTo(ScadNode node)
        {
            BoundNode = node;
            Title = node.NodeTitle;
            HintTooltip = node.NodeDescription;
            RectMinSize = new Vector2(200, 120);
            Offset = node.Offset;

            var maxPorts = Mathf.Max(node.InputPorts.Count, node.OutputPorts.Count);

            var idx = 0;
            while (idx < maxPorts)
            {
                var container = new HBoxContainer();
                AddChild(container);

                if (node.InputPorts.Count > idx)
                {
                    BuildPort(container, idx, node.InputPorts[idx], true, node);
                }

                if (node.OutputPorts.Count > idx)
                {
                    BuildPort(container, idx, node.OutputPorts[idx], false, node);
                }

                idx++;
            }
        }

        public void PortConnected(int port, bool isLeft)
        {
            if (isLeft)
            {
                if (_inputLiteralWidgets.TryGetValue(port, out var widget))
                {
                    widget.SetEnabled(false);
                }
            }
        }

        public void PortDisconnected(int port, bool isLeft)
        {
            if (isLeft)
            {
                if (_inputLiteralWidgets.TryGetValue(port, out var widget))
                {
                    widget.SetEnabled(true);
                }
            }
        }

        private void BuildPort(Container container, int idx, PortDefinition portDefinition, bool isLeft, ScadNode node)
        {
            var connectorPortType = portDefinition.AutoCoerce ? PortType.Any : portDefinition.PortType;
            if (isLeft)
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

            IScadLiteralWidget literalWidget = null;

            var literal = isLeft ? node.GetInputLiteral(idx) : node.GetOutputLiteral(idx);
            switch (literal)
            {
                case BooleanLiteral booleanLiteral:
                    var booleanEdit = Prefabs.New<BooleanEdit>();
                    booleanEdit.BindTo(booleanLiteral);
                    literalWidget = booleanEdit;
                    break;

                case NumberLiteral numberLiteral:
                    var numberEdit = Prefabs.New<NumberEdit>();
                    numberEdit.BindTo(numberLiteral);
                    literalWidget = numberEdit;
                    break;

                case StringLiteral stringLiteral:
                    var stringEdit = Prefabs.New<StringEdit>();
                    stringEdit.BindTo(stringLiteral);
                    literalWidget = stringEdit;
                    break;

                case Vector3Literal vector3Literal:
                    var vector3Edit = Prefabs.New<Vector3Edit>();
                    vector3Edit.BindTo(vector3Literal);
                    literalWidget = vector3Edit;
                    break;
            }

            if (literalWidget != null)
            {
                if (isLeft)
                {
                    _inputLiteralWidgets[idx] = literalWidget;
                }
                else
                {
                    _outputLiteralWidgets[idx] = literalWidget;
                }

                literalWidget.ConnectChanged().To(this, nameof(NotifyChanged));

                var portContainer = Prefabs.InstantiateFromScene<PortContainer.PortContainer>();
                portContainer.MoveToNewParent(container);
                portContainer.Setup(isLeft, portDefinition.Name, (Control) literalWidget);
            }
        }


        private Color ColorFor(PortType portType)
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
                case PortType.Array:
                    return new Color(.2f, 0.2f, 1f);
                case PortType.String:
                    return new Color(1f, 1f, 0f);
                case PortType.Range:
                    return new Color(.2f, 0.2f, 1f);
                case PortType.Any:
                    return new Color(1, 0f, 1f);
                default:
                    throw new ArgumentOutOfRangeException(nameof(portType), portType, null);
            }
        }

        public ConnectExt.ConnectBinding ConnectChanged()
        {
            return this.Connect(nameof(Changed));
        }

        private void NotifyChanged()
        {
            EmitSignal(nameof(Changed));
        }
    }
}